using DataAccess.Data;
using ForagerSite.DataContainer;
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

    public SessionUserState? AuthenticateUser(string username, string password)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var userSecurity = context.UserSecurities
                .Include(us => us.User)
                .FirstOrDefault(us => us.UssUsername == username && us.UssPassword == password);

            if (userSecurity != null)
            {
                return new SessionUserState
                {
                    IsAuthenticated = true,
                    UserId = userSecurity.User.UsrId,
                    Username = userSecurity.UssUsername,
                    DisplayName = userSecurity.User.UsrName ?? string.Empty
                };
            }
            return null;
        }
    }
    //public async Task Load()
    //{
    //    var json = await _session.GetAsync<string>("SessionUser");
    //    if (json.Success && !string.IsNullOrWhiteSpace(json.Value))
    //    {
    //        SessionUser = JsonConvert.DeserializeObject<SessionUserState>(json.Value);
    //        IsAuthenticated = SessionUser?.IsAuthenticated == true;
    //    }
    //    else
    //    {
    //        SessionUser = null;
    //        IsAuthenticated = false;
    //    }

    //    OnChange?.Invoke();
    //}
    public async Task Load()
    {
        try
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
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("statically rendered"))
        {
            // We are prerendering; JS interop is not available yet.
            // Do NOT call DeleteAsync here.
            SessionUser = null;
            IsAuthenticated = false;
            return;
        }

        OnChange?.Invoke();
    }


    public async Task SetUserState(bool isAuthenticated, SessionUserState? state)
    {
        IsAuthenticated = isAuthenticated;

        if (!isAuthenticated || state is null || state.UserId == Guid.Empty)
        {
            SessionUser = null;
            await _session.DeleteAsync("SessionUser");
            OnChange?.Invoke();
            return;
        }

        SessionUser = new SessionUserState
        {
            IsAuthenticated = true,
            UserId = state.UserId,
            Username = state.Username,
            DisplayName = state.DisplayName
        };

        await _session.SetAsync("SessionUser", JsonConvert.SerializeObject(SessionUser));
        OnChange?.Invoke();
    }

    public Task Logout() => SetUserState(false, null);
}
