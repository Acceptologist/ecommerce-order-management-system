using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Application.Features.Products.Commands.Delete;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _memoryCache;

    public DeleteProductCommandHandler(IUnitOfWork uow, IMemoryCache memoryCache)
    {
        _uow = uow;
        _memoryCache = memoryCache;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<Product>();
        var existing = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing != null && !existing.IsDeleted)
        {
            existing.IsDeleted = true;
            existing.DeletedAtUtc = DateTime.UtcNow;
            repo.Update(existing);
            await _uow.SaveChangesAsync(cancellationToken);
            var current = (int?)(_memoryCache.Get("products_list_version") ?? 0) ?? 0;
            _memoryCache.Set("products_list_version", current + 1, TimeSpan.FromDays(1));
        }

        return Unit.Value;
    }
}


