using ForagerSite.DataContainer;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;
using ForagerSite.Services;
using ForagerSite.Services.Interfaces;

public class UserSessionService : IUserSessionService
{
    private readonly ProtectedSessionStorage _session;

    public bool IsAuthenticated { get; private set; }
    public SessionUserState? SessionUser { get; private set; }

    public event Action? OnChange;

    public UserSessionService(ProtectedSessionStorage session)
        => _session = session;

    public async Task Load()
    {
        var json = await _session.GetAsync<string>("SessionUser");
        if (json.Success && !string.IsNullOrWhiteSpace(json.Value))
        {
            SessionUser = JsonConvert.DeserializeObject<SessionUserState>(json.Value);
            IsAuthenticated = SessionUser?.IsAuthenticated == true;
        }
        else
        {
            SessionUser = null;
            IsAuthenticated = false;
        }

        OnChange?.Invoke();
    }

    public async Task SetUserState(bool isAuthenticated, UserDataContainer? vm)
    {
        IsAuthenticated = isAuthenticated;

        if (!isAuthenticated || vm?.user == null || vm.userSecurity == null)
        {
            SessionUser = null;
            await _session.DeleteAsync("SessionUser");
            OnChange?.Invoke();
            return;
        }

        SessionUser = new SessionUserState
        {
            IsAuthenticated = true,
            UserId = vm.user.UsrId,
            Username = vm.userSecurity.UssUsername,
            DisplayName = vm.user.UsrName ?? ""
        };

        await _session.SetAsync("SessionUser", JsonConvert.SerializeObject(SessionUser));
        OnChange?.Invoke();
    }

    public Task Logout() => SetUserState(false, null);
}
