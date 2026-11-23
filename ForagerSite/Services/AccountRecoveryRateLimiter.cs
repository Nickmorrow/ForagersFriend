using System.Security.Cryptography;
using System.Text;

namespace ForagerSite.Services;

public sealed class AccountRecoveryRateLimiter
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);
    private const int PermitLimit = 3;
    private const int MaximumPartitions = 10000;
    private readonly Dictionary<string, Partition> _partitions = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    public bool TryAcquire(string? requester, string? email)
    {
        var normalizedEmail = (email ?? string.Empty).Trim().ToUpperInvariant();
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{requester}|{normalizedEmail}")));
        var now = DateTime.UtcNow;

        lock (_gate)
        {
            foreach (var expired in _partitions.Where(item => item.Value.ExpiresUtc <= now).Select(item => item.Key).ToArray())
                _partitions.Remove(expired);

            if (!_partitions.TryGetValue(key, out var partition))
            {
                if (_partitions.Count >= MaximumPartitions)
                    return false;

                partition = new Partition(now.Add(Window));
                _partitions.Add(key, partition);
            }

            if (partition.Count >= PermitLimit)
                return false;

            partition.Count++;
            return true;
        }
    }

    private sealed class Partition(DateTime expiresUtc)
    {
        public DateTime ExpiresUtc { get; } = expiresUtc;
        public int Count { get; set; }
    }
}
