using DataAccess.Models;
using ForagerSite;
using ForagerSite.DataContainer;
using ForagerSite.Services;
using ForagerSite.Services.Interfaces;
namespace ForagerSite.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> UsernameExists(string username);
        Task<bool> EmailExists(string email, User? user = null);
        void UpdateUserSecurity(UserSecurity userSecurity);
        void AddUser(User user, UserSecurity userSecurity);
        Task<User?> GetUser(Guid userId);
        Task<List<User>> GetAllUsers();
        Task<List<string>> GetAllUserNames();
        Task UpdateUser(User user);
        Task<string> GetUserProfilePic(User user);
        Task UploadProfilePicUrl(User user, string fileUrl);
        Task DeleteProfilePicUrl(User user);
        Task<UserDataContainer> GetUserViewModelById(Guid userId);
    }
}
