namespace ForagerSite.Services;

public interface IAccountEmailSender
{
    Task SendConfirmationAsync(string emailAddress, string confirmationUrl, CancellationToken cancellationToken);
    Task SendAsync(string emailAddress, string subject, string body, CancellationToken cancellationToken);
}
