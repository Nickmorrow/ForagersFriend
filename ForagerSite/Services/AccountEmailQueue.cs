using DataAccess.Data;
using DataAccess.Models;

namespace ForagerSite.Services;

public sealed class AccountEmailQueue : IAccountEmailQueue
{
    private readonly ForagerDbContext _db;

    public AccountEmailQueue(ForagerDbContext db)
    {
        _db = db;
    }

    public async Task EnqueueAsync(
        AccountEmailJobType type,
        string? email = null,
        Guid? accountId = null,
        string? newEmail = null,
        CancellationToken cancellationToken = default)
    {
        _db.AccountEmailJobs.Add(new AccountEmailJob
        {
            Type = type,
            Email = email?.Trim(),
            AccountId = accountId,
            NewEmail = newEmail?.Trim(),
            CreatedUtc = DateTime.UtcNow,
            NextAttemptUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
