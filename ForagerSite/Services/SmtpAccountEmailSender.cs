using System.Net;
using System.Net.Mail;

namespace ForagerSite.Services;

public sealed class SmtpAccountEmailSender : IAccountEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpAccountEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendConfirmationAsync(
        string emailAddress,
        string confirmationUrl,
        CancellationToken cancellationToken)
    {
        await SendAsync(
            emailAddress,
            "Confirm your WildFinds account",
            $"Confirm your account by opening this link:\r\n\r\n{confirmationUrl}",
            cancellationToken);
    }

    public async Task SendAsync(
        string emailAddress,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var settings = _configuration.GetSection("AccountEmail:Smtp");
        var host = settings["Host"];
        var fromAddress = settings["FromAddress"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromAddress))
            throw new InvalidOperationException("SMTP host and sender address must be configured.");

        var port = settings.GetValue<int?>("Port") ?? 587;
        var username = settings["Username"];
        var password = settings["Password"];
        using var message = new MailMessage(fromAddress, emailAddress)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.From = new MailAddress(fromAddress, settings["DisplayName"] ?? "WildFinds");

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = settings.GetValue("EnableSsl", true),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };
        if (!string.IsNullOrWhiteSpace(username))
            client.Credentials = new NetworkCredential(username, password);

        await client.SendMailAsync(message, cancellationToken);
    }
}
