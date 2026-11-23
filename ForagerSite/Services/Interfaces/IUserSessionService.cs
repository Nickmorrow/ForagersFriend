using ForagerSite.DataContainer;

namespace ForagerSite.Services.Interfaces
{
    public interface IUserSessionService
    {
        SessionUserState? SessionUser { get; }
        bool IsAuthenticated { get; }
        event Action? OnChange;
        Task Load();






    }
}
