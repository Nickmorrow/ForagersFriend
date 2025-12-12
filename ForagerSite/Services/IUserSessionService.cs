using ForagerSite.DataContainer;

namespace ForagerSite.Services
{
    public interface IUserSessionService
    {
        Task Load();
        Task SetUserState(bool isAuthenticated, UserDataContainer? vm);
        Task Logout();

    }
}
