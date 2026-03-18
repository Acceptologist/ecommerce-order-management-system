namespace ECommerce.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(string userId, string username, IEnumerable<string> roles);
    string GenerateRefreshToken();
}

