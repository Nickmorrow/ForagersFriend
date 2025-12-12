using ForagerSite;
using ForagerSite.Services;
using ForagerSite.Services.Interfaces;
using System.Threading.Tasks;

namespace ForagerSite.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmail(string email, string resetLink);
    }
}
