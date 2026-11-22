using ForagerSite.DataContainer;

namespace ForagerSite.Services.Interfaces
{
    public interface IUserSessionService
    {
        SessionUserState? SessionUser { get; }
        bool IsAuthenticated { get; }
        event Action? OnChange;
        Task Load();
        Task Logout();
        Task SetUserState(bool isAuthenticated, SessionUserState? state);
        SessionUserState? AuthenticateUser(string username, string password);





    }
}
