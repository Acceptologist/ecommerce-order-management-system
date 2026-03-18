using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyDictionary<int, string>> GetUserNamesByIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<int, string>();

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync(cancellationToken);

        return users.Where(u => u.UserName != null).ToDictionary(u => u.Id, u => u.UserName!);
    }

    public async Task<IEnumerable<int>> SearchUserIdsByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return Enumerable.Empty<int>();

        var term = searchTerm.ToLower();
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.UserName != null && u.UserName.ToLower().Contains(term))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }
}
