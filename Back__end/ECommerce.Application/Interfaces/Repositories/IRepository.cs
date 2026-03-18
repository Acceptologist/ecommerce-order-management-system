namespace ECommerce.Application.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    IQueryable<T> Query();
    Task<List<T>> ToListAsync(IQueryable<T> query, CancellationToken cancellationToken = default);
    Task<int> CountAsync(IQueryable<T> query, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}

