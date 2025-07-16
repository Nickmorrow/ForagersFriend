using System.Threading.Tasks;

namespace ForagerSite.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmail(string email, string resetLink);
    }
}
