namespace ECommerce.Application.Interfaces.Services;

/// <summary>
/// Stores revoked JWT tokens until they expire (logout / security).
/// </summary>
public interface IRevokedTokenStore
{
    void Revoke(string token);
    bool IsRevoked(string token);
}
