namespace ECommerce.Application.Interfaces.Persistence;

public interface IUserRepository
{
    Task<IReadOnlyDictionary<int, string>> GetUserNamesByIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);
    Task<IEnumerable<int>> SearchUserIdsByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
}
