namespace ForagerSite.Services;

public interface IAccountEmailSender
{
    Task SendConfirmationAsync(string emailAddress, string confirmationUrl, CancellationToken cancellationToken);
}
