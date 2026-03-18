using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands.Delete;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IUnitOfWork _uow;

    public DeleteCategoryCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<Category>();
        var existing = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null || existing.IsDeleted)
        {
            return Unit.Value;
        }

        // business rule: cannot delete category that still has products
        var productRepo = _uow.Repository<Product>();
        var productsQuery = productRepo.Query().Where(p => p.CategoryId == existing.Id && !p.IsDeleted);
        var hasAnyProducts = await productRepo.CountAsync(productsQuery.Take(1), cancellationToken) > 0;
        if (hasAnyProducts)
        {
            throw new InvalidOperationException("Cannot delete a category that has products. Move or delete products first.");
        }

        existing.IsDeleted = true;
        existing.DeletedAtUtc = DateTime.UtcNow;
        repo.Update(existing);
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

