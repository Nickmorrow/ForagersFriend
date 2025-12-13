using DataAccess.Data;
using ForagerSite.DataContainer;
using ForagerSite.Services;
using ForagerSite.Services.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;

public class UserSessionService : IUserSessionService
{
    private readonly ProtectedSessionStorage _session;
    private IDbContextFactory<ForagerDbContext> _dbContextFactory;
    public bool IsAuthenticated { get; private set; }
    public SessionUserState? SessionUser { get; private set; }

    public event Action? OnChange;

    public UserSessionService(ProtectedSessionStorage session, IDbContextFactory<ForagerDbContext> dbContextFactory)
    {
        _session = session;
        _dbContextFactory = dbContextFactory;
    }       

    public UserDataContainer AuthenticateUser(string username, string password)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var userSecurity = context.UserSecurities
                .Include(us => us.User)
                .FirstOrDefault(us => us.UssUsername == username && us.UssPassword == password);

            if (userSecurity != null)
            {
                return new UserDataContainer
                {
                    user = userSecurity.User,
                    userSecurity = userSecurity
                };
            }
            return null;
        }
    }
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
