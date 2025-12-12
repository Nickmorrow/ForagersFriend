using ForagerSite;
using ForagerSite.DataContainer;
using ForagerSite.Services;
using ForagerSite.Services.Interfaces;

namespace ForagerSite.Services.Interfaces
{
    public interface IUserSessionService
    {
        Task Load();
        Task SetUserState(bool isAuthenticated, UserDataContainer? vm);
        Task Logout();

    }
}
