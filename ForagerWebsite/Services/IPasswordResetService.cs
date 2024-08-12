using System.Threading.Tasks;
using ForagerWebSite.Models;

namespace ForagerWebSite.Services
{
    public interface IPasswordResetService
    {
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);    }
}
