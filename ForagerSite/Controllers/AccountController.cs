using System.Text;
using System.Text.Encodings.Web;
using DataAccess.Data;
using DataAccess.Models;
using ForagerSite.DataContainer;
using ForagerSite.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace ForagerSite.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly SignInManager<ForagerAccount> _signInManager;
    private readonly UserManager<ForagerAccount> _userManager;
    private readonly ForagerDbContext _db;
    private readonly IAccountEmailSender _emailSender;
    private readonly IAntiforgery _antiforgery;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IAccountEmailQueue _emailQueue;
    private readonly AccountRecoveryRateLimiter _recoveryRateLimiter;
    private readonly ILogger<AccountController> _logger;
    private bool RequireConfirmedEmail => !_environment.IsDevelopment() ||
        _configuration.GetValue<bool>("Authentication:RequireConfirmedEmail");

    public AccountController(
        SignInManager<ForagerAccount> signInManager,
        UserManager<ForagerAccount> userManager,
        ForagerDbContext db,
        IAccountEmailSender emailSender,
        IAntiforgery antiforgery,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IAccountEmailQueue emailQueue,
        AccountRecoveryRateLimiter recoveryRateLimiter,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
        _emailSender = emailSender;
        _antiforgery = antiforgery;
        _configuration = configuration;
        _environment = environment;
        _emailQueue = emailQueue;
        _recoveryRateLimiter = recoveryRateLimiter;
        _logger = logger;
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("account-login")]
    public async Task<IActionResult> Login([FromForm] LoginInputModel input, [FromForm] string? returnUrl)
    {
        if (!ModelState.IsValid)
            return Redirect("/Login?status=invalid");

        var result = await _signInManager.PasswordSignInAsync(
            input.Username.Trim(), input.Password, isPersistent: false, lockoutOnFailure: true);

        if (!result.Succeeded)
            return Redirect(result.IsLockedOut ? "/Login?status=locked" : "/Login?status=invalid");

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }

    [HttpPost("recover")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("account-recovery")]
    public async Task<IActionResult> RequestRecovery(
        [FromForm] AccountRecoveryInputModel input,
        [FromForm] string? returnUrl)
    {
        if (ModelState.IsValid && _recoveryRateLimiter.TryAcquire(
            HttpContext.Connection.RemoteIpAddress?.ToString(), input.Email))
        {
            var jobType = input.RequestType switch
            {
                "password" => AccountEmailJobType.PasswordReset,
                "username" => AccountEmailJobType.UserNameRecovery,
                "confirmation" => AccountEmailJobType.ConfirmationResend,
                _ => (AccountEmailJobType?)null
            };

            if (jobType is not null)
            {
                try
                {
                    await _emailQueue.EnqueueAsync(jobType.Value, email: input.Email, cancellationToken: HttpContext.RequestAborted);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning("Unable to enqueue an account recovery email.");
                }
            }
        }

        var destination = Url.IsLocalUrl(returnUrl)
            ? returnUrl!
            : "/RecoverLoginInfo?status=acknowledged";
        return LocalRedirect(destination);
    }

    [HttpGet("reset-password")]
    public IActionResult ResetPasswordForm([FromQuery] string? userId, [FromQuery] string? token)
    {
        Response.Headers["Cache-Control"] = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        if (!Guid.TryParse(userId, out _) || string.IsNullOrWhiteSpace(token))
            return Content("<p>This password reset link is invalid or expired.</p>", "text/html; charset=utf-8");

        var antiforgeryTokens = _antiforgery.GetAndStoreTokens(HttpContext);
        var htmlToken = HtmlEncoder.Default.Encode(token);
        var htmlUserId = HtmlEncoder.Default.Encode(userId);
        var htmlRequestToken = HtmlEncoder.Default.Encode(antiforgeryTokens.RequestToken ?? string.Empty);
        var html = $"""
            <!doctype html><html lang="en"><head><meta charset="utf-8"><title>Reset password</title></head>
            <body><h1>Reset password</h1><form method="post" action="/account/reset-password">
            <input type="hidden" name="__RequestVerificationToken" value="{htmlRequestToken}">
            <input type="hidden" name="UserId" value="{htmlUserId}">
            <input type="hidden" name="Token" value="{htmlToken}">
            <label for="Password">New password</label><input id="Password" name="Password" type="password" minlength="8" maxlength="256" required>
            <label for="ConfirmPassword">Confirm new password</label><input id="ConfirmPassword" name="ConfirmPassword" type="password" maxlength="256" required>
            <button type="submit">Reset password</button></form></body></html>
            """;
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpPost("reset-password")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("account-recovery")]
    public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordInputModel input)
    {
        if (!ModelState.IsValid || !Guid.TryParse(input.UserId, out var accountId))
            return Redirect("/RecoverLoginInfo?status=reset-invalid");

        var account = await _userManager.FindByIdAsync(accountId.ToString());
        if (account is null || !await _userManager.IsEmailConfirmedAsync(account))
            return Redirect("/RecoverLoginInfo?status=reset-invalid");

        IdentityResult result;
        try
        {
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(input.Token));
            result = await _userManager.ResetPasswordAsync(account, token, input.Password);
        }
        catch (FormatException)
        {
            return Redirect("/RecoverLoginInfo?status=reset-invalid");
        }

        if (!result.Succeeded)
            return Redirect("/RecoverLoginInfo?status=reset-invalid");

        try
        {
            await _emailQueue.EnqueueAsync(
                AccountEmailJobType.PasswordChangedNotice,
                accountId: account.Id,
                cancellationToken: HttpContext.RequestAborted);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning("Unable to enqueue a password-change notification.");
        }

        return Redirect("/Login?status=password-reset");
    }

    [HttpPost("email-change")]
    [Authorize]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("account-email-change")]
    public async Task<IActionResult> RequestEmailChange([FromForm] EmailChangeInputModel input)
    {
        if (!ModelState.IsValid)
            return Redirect("/Account/Email?status=invalid");

        var account = await _userManager.GetUserAsync(User);
        if (account is null)
            return Challenge();

        var newEmail = input.NewEmail.Trim();
        if (string.Equals(account.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            return Redirect("/Account/Email?status=same-email");

        var existingAccount = await _userManager.FindByEmailAsync(newEmail);
        if (existingAccount is not null && existingAccount.Id != account.Id)
            return Redirect("/Account/Email?status=unavailable");

        try
        {
            await _emailQueue.EnqueueAsync(
                AccountEmailJobType.EmailChangeConfirmation,
                accountId: account.Id,
                newEmail: newEmail,
                cancellationToken: HttpContext.RequestAborted);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning("Unable to enqueue an email-change confirmation.");
            return Redirect("/Account/Email?status=unavailable");
        }

        return Redirect("/Account/Email?status=confirmation-sent");
    }

    [HttpGet("confirm-email-change")]
    [Authorize]
    public async Task<IActionResult> ConfirmEmailChangeForm(
        [FromQuery] string? userId,
        [FromQuery] string? email,
        [FromQuery] string? token)
    {
        Response.Headers["Cache-Control"] = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        var account = await _userManager.GetUserAsync(User);
        if (account is null || !Guid.TryParse(userId, out var requestedId) || requestedId != account.Id ||
            string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return Content("<p>This email-change link is invalid or expired.</p>", "text/html; charset=utf-8");

        var antiforgeryTokens = _antiforgery.GetAndStoreTokens(HttpContext);
        var htmlUserId = HtmlEncoder.Default.Encode(userId!);
        var htmlEmail = HtmlEncoder.Default.Encode(email);
        var htmlToken = HtmlEncoder.Default.Encode(token);
        var htmlRequestToken = HtmlEncoder.Default.Encode(antiforgeryTokens.RequestToken ?? string.Empty);
        var html = $"""
            <!doctype html><html lang="en"><head><meta charset="utf-8"><title>Confirm email change</title></head>
            <body><h1>Confirm email change</h1><p>Confirm the new address {htmlEmail}.</p>
            <form method="post" action="/account/confirm-email-change">
            <input type="hidden" name="__RequestVerificationToken" value="{htmlRequestToken}">
            <input type="hidden" name="UserId" value="{htmlUserId}">
            <input type="hidden" name="Email" value="{htmlEmail}">
            <input type="hidden" name="Token" value="{htmlToken}">
            <button type="submit">Confirm new email</button></form></body></html>
            """;
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpPost("confirm-email-change")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmEmailChange([FromForm] ConfirmEmailChangeInputModel input)
    {
        if (!ModelState.IsValid || !Guid.TryParse(input.UserId, out var accountId))
            return Redirect("/Account/Email?status=confirmation-invalid");

        var account = await _userManager.GetUserAsync(User);
        if (account is null)
            return Challenge();
        if (account.Id != accountId)
            return Redirect("/Account/Email?status=confirmation-invalid");

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(input.Token));
        }
        catch (FormatException)
        {
            return Redirect("/Account/Email?status=confirmation-invalid");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var result = await _userManager.ChangeEmailAsync(account, input.Email.Trim(), token);
        if (!result.Succeeded)
        {
            await transaction.RollbackAsync();
            return Redirect("/Account/Email?status=confirmation-invalid");
        }

        var profile = await _db.Profiles.SingleOrDefaultAsync(item => item.UsrId == account.Id);
        if (profile is not null)
            profile.UsrEmail = account.Email!;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        await _signInManager.RefreshSignInAsync(account);

        return Redirect("/Account/Email?status=updated");
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("account-registration")]
    public async Task<IActionResult> Register([FromForm] RegistrationInputModel input)
    {
        if (!ModelState.IsValid)
        {
            var validationErrors = ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .Select(entry => entry.Key.Split('.').Last())
                .Select(MapModelFieldError);
            return RedirectWithRegistrationErrors(validationErrors);
        }

        var accountId = Guid.NewGuid();
        var profile = new User
        {
            UsrId = accountId,
            UsrName = input.Name.Trim(),
            UsrEmail = input.Email.Trim(),
            UsrCountry = input.Country?.Trim(),
            UsrStateorProvince = input.StateOrProvince?.Trim(),
            UsrJoinedDate = DateTime.UtcNow
        };
        var account = new ForagerAccount
        {
            Id = accountId,
            UserName = input.Username.Trim(),
            Email = input.Email.Trim(),
            EmailConfirmed = false,
            LockoutEnabled = true,
            Profile = profile
        };

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Profiles.Add(profile);
            var createResult = await _userManager.CreateAsync(account, input.Password);
            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = createResult.Errors.Any(error =>
                    error.Code is "DuplicateEmail" or "DuplicateUserName")
                    ? new[] { "account-details" }
                    : createResult.Errors.Select(MapIdentityError);
                return RedirectWithRegistrationErrors(errors);
            }

            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            return RedirectWithRegistrationErrors(new[] { "account-details" });
        }

        if (!RequireConfirmedEmail)
            return Redirect("/Registration?status=development-created");

        try
        {
            var confirmationUrl = await CreateConfirmationUrlAsync(account);
            await _emailSender.SendConfirmationAsync(account.Email!, confirmationUrl, HttpContext.RequestAborted);
            return Redirect("/Registration?status=confirmation-sent");
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.Net.Mail.SmtpException)
        {
            return Redirect("/Registration?status=confirmation-unavailable");
        }
    }

    [HttpGet("confirm")]
    public IActionResult ConfirmEmailForm([FromQuery] string? userId, [FromQuery] string? token)
    {
        Response.Headers["Cache-Control"] = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        if (!Guid.TryParse(userId, out _) || string.IsNullOrWhiteSpace(token))
            return Content("<p>This confirmation link is invalid or expired.</p>", "text/html");

        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        var encodedUserId = HtmlEncoder.Default.Encode(userId);
        var encodedToken = HtmlEncoder.Default.Encode(token);
        var encodedRequestToken = HtmlEncoder.Default.Encode(tokens.RequestToken ?? string.Empty);
        var html = $"""
            <!doctype html><html lang="en"><head><meta charset="utf-8"><title>Confirm account</title></head>
            <body><h1>Confirm account</h1><form method="post" action="/account/confirm">
            <input type="hidden" name="__RequestVerificationToken" value="{encodedRequestToken}">
            <input type="hidden" name="userId" value="{encodedUserId}">
            <input type="hidden" name="token" value="{encodedToken}">
            <button type="submit">Confirm email</button></form></body></html>
            """;
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpPost("confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmEmail([FromForm] string userId, [FromForm] string token)
    {
        if (!Guid.TryParse(userId, out var accountId))
            return Redirect("/Login?status=confirmation-invalid");

        var account = await _userManager.FindByIdAsync(accountId.ToString());
        if (account is null)
            return Redirect("/Login?status=confirmation-invalid");

        try
        {
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(account, decodedToken);
            return Redirect(result.Succeeded ? "/Login?status=confirmed" : "/Login?status=confirmation-invalid");
        }
        catch (FormatException)
        {
            return Redirect("/Login?status=confirmation-invalid");
        }
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout([FromForm] string? returnUrl)
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }

    private async Task<string> CreateConfirmationUrlAsync(ForagerAccount account)
    {
        var origin = _configuration["AccountEmail:PublicOrigin"];
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("A public HTTPS account origin must be configured.");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(account);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        return $"{origin!.TrimEnd('/')}/account/confirm?userId={Uri.EscapeDataString(account.Id.ToString())}&token={Uri.EscapeDataString(encodedToken)}";
    }

    private IActionResult RedirectWithRegistrationErrors(IEnumerable<string> errors)
    {
        var errorCodes = errors.Distinct().ToArray();
        if (errorCodes.Length == 0)
            errorCodes = new[] { "form" };

        return Redirect($"/Registration?errors={Uri.EscapeDataString(string.Join(',', errorCodes))}");
    }

    private static string MapModelFieldError(string field) => field switch
    {
        nameof(RegistrationInputModel.Name) => "name",
        nameof(RegistrationInputModel.Email) => "email",
        nameof(RegistrationInputModel.Country) => "country",
        nameof(RegistrationInputModel.StateOrProvince) => "state",
        nameof(RegistrationInputModel.Username) => "username",
        nameof(RegistrationInputModel.Password) => "password",
        nameof(RegistrationInputModel.ConfirmPassword) => "confirm-password",
        _ => "form"
    };

    private static string MapIdentityError(IdentityError error) => error.Code switch
    {
        "InvalidEmail" => "email",
        "InvalidUserName" => "username",
        _ when error.Code.StartsWith("Password", StringComparison.Ordinal) => "password",
        _ => "form"
    };
}
