using ECommerce.Application.Interfaces.Repositories;

namespace ECommerce.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

