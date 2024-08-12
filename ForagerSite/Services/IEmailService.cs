using System.Threading.Tasks;

namespace ForagerSite.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetLink);
    }
}
