using System.Text;
using DataAccess.Data;
using DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace ForagerSite.Services;

public sealed class AccountEmailWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan JobLease = TimeSpan.FromMinutes(5);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AccountEmailWorker> _logger;
    private DateTime _nextPendingEmailCleanupUtc;

    public AccountEmailWorker(IServiceScopeFactory scopeFactory, ILogger<AccountEmailWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!await ProcessNextAsync(stoppingToken))
                    await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                _logger.LogWarning("Account email queue processing failed.");
                await Task.Delay(PollInterval, stoppingToken);
            }
        }
    }

    private async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ForagerDbContext>();
        var now = DateTime.UtcNow;
        if (_nextPendingEmailCleanupUtc <= now)
        {
            await db.AccountEmailJobs
                .Where(item => item.Type == AccountEmailJobType.EmailChangeConfirmation &&
                    item.NextAttemptUtc == DateTime.MaxValue && item.CreatedUtc <= now.AddHours(-1))
                .ExecuteDeleteAsync(cancellationToken);
            _nextPendingEmailCleanupUtc = now.AddMinutes(10);
        }

        var job = await db.AccountEmailJobs
            .Where(item => item.NextAttemptUtc <= now &&
                (item.LockedUntilUtc == null || item.LockedUntilUtc <= now))
            .OrderBy(item => item.CreatedUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
            return false;

        var leaseUntil = now.Add(JobLease);
        var claimed = await db.AccountEmailJobs
            .Where(item => item.Id == job.Id &&
                (item.LockedUntilUtc == null || item.LockedUntilUtc <= now))
            .ExecuteUpdateAsync(update => update
                .SetProperty(item => item.LockedUntilUtc, leaseUntil)
                .SetProperty(item => item.Attempts, item => item.Attempts + 1), cancellationToken);

        if (claimed == 0)
            return true;

        await db.Entry(job).ReloadAsync(cancellationToken);

        try
        {
            var retainForConfirmation = await SendAsync(scope.ServiceProvider, job, cancellationToken);
            if (retainForConfirmation)
            {
                job.CreatedUtc = DateTime.UtcNow;
                job.LockedUntilUtc = null;
                job.NextAttemptUtc = DateTime.MaxValue;
            }
            else
            {
                db.AccountEmailJobs.Remove(job);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            _logger.LogWarning("Account email job {JobId} failed.", job.Id);
            if (job.Attempts >= 5)
            {
                db.AccountEmailJobs.Remove(job);
            }
            else
            {
                job.LockedUntilUtc = null;
                job.NextAttemptUtc = DateTime.UtcNow.AddSeconds(Math.Min(60 * job.Attempts, 1800));
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    private static async Task<bool> SendAsync(IServiceProvider services, AccountEmailJob job, CancellationToken cancellationToken)
    {
        var userManager = services.GetRequiredService<UserManager<ForagerAccount>>();
        var sender = services.GetRequiredService<IAccountEmailSender>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var origin = GetPublicOrigin(configuration);
        ForagerAccount? account;
        string? recipient;
        string subject;
        string body;

        switch (job.Type)
        {
            case AccountEmailJobType.PasswordReset:
                account = await userManager.FindByEmailAsync(job.Email ?? string.Empty);
                if (account is null || !await userManager.IsEmailConfirmedAsync(account))
                    return false;
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(account);
                var resetUrl = BuildUrl(origin, "/account/reset-password", new Dictionary<string, string?>
                {
                    ["userId"] = account.Id.ToString(),
                    ["token"] = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken))
                });
                recipient = account.Email;
                subject = "Reset your ForagersFriend password";
                body = $"Reset your password by opening this link:\r\n\r\n{resetUrl}";
                break;

            case AccountEmailJobType.UserNameRecovery:
                account = await userManager.FindByEmailAsync(job.Email ?? string.Empty);
                if (account is null || !await userManager.IsEmailConfirmedAsync(account) || string.IsNullOrWhiteSpace(account.UserName))
                    return false;
                recipient = account.Email;
                subject = "Your ForagersFriend username";
                body = $"Your username is: {account.UserName}";
                break;

            case AccountEmailJobType.ConfirmationResend:
                account = await userManager.FindByEmailAsync(job.Email ?? string.Empty);
                if (account is null || await userManager.IsEmailConfirmedAsync(account))
                    return false;
                var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(account);
                var confirmationUrl = BuildUrl(origin, "/account/confirm", new Dictionary<string, string?>
                {
                    ["userId"] = account.Id.ToString(),
                    ["token"] = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken))
                });
                recipient = account.Email;
                subject = "Confirm your ForagersFriend account";
                body = $"Confirm your account by opening this link:\r\n\r\n{confirmationUrl}";
                break;

            case AccountEmailJobType.PasswordChangedNotice:
                account = job.AccountId is Guid accountId
                    ? await userManager.FindByIdAsync(accountId.ToString())
                    : null;
                if (account is null || !await userManager.IsEmailConfirmedAsync(account))
                    return false;
                recipient = account.Email;
                subject = "Your ForagersFriend password was changed";
                body = "Your ForagersFriend account password was changed. If you did not make this change, contact support immediately.";
                break;

            case AccountEmailJobType.EmailChangeConfirmation:
                account = job.AccountId is Guid emailAccountId
                    ? await userManager.FindByIdAsync(emailAccountId.ToString())
                    : null;
                if (account is null || string.IsNullOrWhiteSpace(job.NewEmail))
                    return false;
                var emailChangeToken = await userManager.GenerateChangeEmailTokenAsync(account, job.NewEmail);
                var emailChangeUrl = BuildUrl(origin, "/account/confirm-email-change", new Dictionary<string, string?>
                {
                    ["jobId"] = job.Id.ToString(),
                    ["token"] = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailChangeToken))
                });
                recipient = job.NewEmail;
                subject = "Confirm your ForagersFriend email change";
                body = $"Confirm your new email address by opening this link:\r\n\r\n{emailChangeUrl}";
                break;

            default:
                return false;
        }

        if (!string.IsNullOrWhiteSpace(recipient))
            await sender.SendAsync(recipient, subject, body, cancellationToken);

        return job.Type == AccountEmailJobType.EmailChangeConfirmation && !string.IsNullOrWhiteSpace(recipient);
    }

    private static string GetPublicOrigin(IConfiguration configuration)
    {
        var origin = configuration["AccountEmail:PublicOrigin"];
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("A public HTTPS account origin must be configured.");

        return origin!.TrimEnd('/');
    }

    private static string BuildUrl(string origin, string path, IDictionary<string, string?> values)
    {
        var query = string.Join("&", values.Select(value =>
            $"{Uri.EscapeDataString(value.Key)}={Uri.EscapeDataString(value.Value ?? string.Empty)}"));
        return $"{origin}{path}?{query}";
    }
}
