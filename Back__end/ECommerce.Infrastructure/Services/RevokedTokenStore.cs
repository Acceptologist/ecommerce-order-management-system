using System.Collections.Concurrent;
using ECommerce.Application.Interfaces.Services;

namespace ECommerce.Infrastructure.Services;

public class RevokedTokenStore : IRevokedTokenStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _revoked = new();

    public void Revoke(string token)
    {
        if (string.IsNullOrEmpty(token)) return;
        _revoked.TryAdd(token.Trim(), DateTimeOffset.UtcNow);
    }

    public bool IsRevoked(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        return _revoked.ContainsKey(token.Trim());
    }
}
