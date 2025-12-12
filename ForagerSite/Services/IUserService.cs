using DataAccess.Models;
using ForagerSite.DataContainer;
namespace ForagerSite.Services
{
    public interface IUserService
    {
        UserViewModel? AuthenticateUser(string username, string password);
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
        Task<UserViewModel> GetUserViewModelById(Guid userId);
    }
}
