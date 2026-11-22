using ForagerSite.DataContainer;
using ForagerSite.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public sealed class UserSessionService : IUserSessionService, IDisposable
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    public bool IsAuthenticated { get; private set; }
    public SessionUserState? SessionUser { get; private set; }

    public event Action? OnChange;

    public UserSessionService(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    public Task Load() => UpdateStateAsync(_authenticationStateProvider.GetAuthenticationStateAsync());

    private void OnAuthenticationStateChanged(Task<AuthenticationState> authenticationStateTask)
    {
        _ = UpdateStateAsync(authenticationStateTask);
    }

    private async Task UpdateStateAsync(Task<AuthenticationState> authenticationStateTask)
    {
        var principal = (await authenticationStateTask).User;
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = principal.Identity?.Name;

        if (principal.Identity?.IsAuthenticated != true ||
            !Guid.TryParse(userId, out var parsedUserId) ||
            parsedUserId == Guid.Empty)
        {
            SessionUser = null;
            IsAuthenticated = false;
        }
        else
        {
            var displayName = principal.FindFirstValue(ClaimTypes.GivenName)
                ?? principal.FindFirstValue(ClaimTypes.Name)
                ?? username
                ?? string.Empty;
            SessionUser = new SessionUserState
            {
                IsAuthenticated = true,
                UserId = parsedUserId,
                Username = username ?? string.Empty,
                DisplayName = displayName
            };
            IsAuthenticated = true;
        }
        OnChange?.Invoke();
    }

    public void Dispose()
    {
        _authenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
