using ForagerSite;
using ForagerSite.DataContainer;
using ForagerSite.Services;
using ForagerSite.Services.Interfaces;

namespace ForagerSite.Services.Interfaces
{
    public interface IUserSessionService
    {
        SessionUserState? SessionUser { get; }
        bool IsAuthenticated { get; }
        event Action? OnChange;
        Task Load();
        Task Logout();
        Task SetUserState(bool isAuthenticated, UserDataContainer? vm);
        UserDataContainer AuthenticateUser(string username, string password);





    }
}
