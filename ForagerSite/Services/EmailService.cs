using ForagerSite.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ForagerSite.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;

        public EmailService(SmtpClient smtpClient)
        {
            _smtpClient = smtpClient;
        }

        public async Task SendPasswordResetEmail(string email, string resetLink)
        {
            var mailMessage = new MailMessage("your-email@example.com", email)
            {
                Subject = "Password Reset",
                Body = $"Please reset your password by clicking the link: {resetLink}"
            };

            await _smtpClient.SendMailAsync(mailMessage);
        }
    }
}
