using System.Security.Claims;
using DataAccess.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ForagerSite.Services;

public sealed class IdentityRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly UserManager<ForagerAccount> _userManager;
    private readonly string _securityStampClaimType;

    public IdentityRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        UserManager<ForagerAccount> userManager,
        IOptions<IdentityOptions> identityOptions)
        : base(loggerFactory)
    {
        _userManager = userManager;
        _securityStampClaimType = identityOptions.Value.ClaimsIdentity.SecurityStampClaimType;
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        var principal = authenticationState.User;
        if (principal.Identity?.IsAuthenticated != true)
            return true;

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var claimedStamp = principal.FindFirstValue(_securityStampClaimType);
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(claimedStamp))
            return false;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return false;

        var currentStamp = await _userManager.GetSecurityStampAsync(user);
        return string.Equals(claimedStamp, currentStamp, StringComparison.Ordinal);
    }
}
