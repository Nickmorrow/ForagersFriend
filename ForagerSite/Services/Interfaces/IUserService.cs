using ForagerSite.DataContainer;
namespace ForagerSite.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> UsernameExists(string username);
        Task<bool> EmailExists(string email);
        Task<List<string>> GetAllUserNames();
        Task UpdateUser(UserProfileDto user);
        Task<string> GetUserProfilePic(Guid userId);
        Task<UserProfileDto> GetUserViewModelById(Guid userId);
    }
}
