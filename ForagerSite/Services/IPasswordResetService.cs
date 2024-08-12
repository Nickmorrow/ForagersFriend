using System.Threading.Tasks;
using ForagerSite.Models;

namespace ForagerSite.Services
{
    public interface IPasswordResetService
    {
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);    }
}
