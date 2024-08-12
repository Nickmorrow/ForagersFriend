using System.Threading.Tasks;

namespace ForagerWebSite.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetLink);
    }
}
