using DataAccess.Models;

namespace ForagerSite.Services;

public interface IAccountEmailQueue
{
    Task EnqueueAsync(
        AccountEmailJobType type,
        string? email = null,
        Guid? accountId = null,
        string? newEmail = null,
        CancellationToken cancellationToken = default);
}
