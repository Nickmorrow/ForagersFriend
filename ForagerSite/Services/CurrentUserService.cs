using System.Security.Claims;
using ForagerSite.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace ForagerSite.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public CurrentUserService(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<Guid> GetRequiredUserIdAsync()
    {
        var principal = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (principal.Identity?.IsAuthenticated != true ||
            !Guid.TryParse(userId, out var parsedUserId) ||
            parsedUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("A signed-in user is required.");
        }

        return parsedUserId;
    }
}
