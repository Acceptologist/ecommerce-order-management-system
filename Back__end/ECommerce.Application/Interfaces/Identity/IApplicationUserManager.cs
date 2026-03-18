namespace ECommerce.Application.Interfaces.Identity;

public interface IApplicationUserManager
{
    Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken = default);
    Task CreateUserAsync(string username, string password, string role, CancellationToken cancellationToken = default);
}

