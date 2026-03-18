using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Application.Features.Products.Commands.Update;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _memoryCache;

    public UpdateProductCommandHandler(IUnitOfWork uow, IMemoryCache memoryCache)
    {
        _uow = uow;
        _memoryCache = memoryCache;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<Product>();
        var existing = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Product with id {request.Id} not found.");
        }

        var dto = request.Dto;

        existing.Name = dto.Name.Trim();
        existing.Description = dto.Description.Trim();
        existing.Price = dto.Price;
        existing.StockQuantity = dto.StockQuantity;
        existing.CategoryId = dto.CategoryId;
        existing.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl!.Trim();
        existing.DiscountRate = dto.DiscountRate;

        repo.Update(existing);
        await _uow.SaveChangesAsync(cancellationToken);

        // Invalidate product list cache so list/grid show updated stock and badges immediately
        var current = (int?)(_memoryCache.Get("products_list_version") ?? 0) ?? 0;
        _memoryCache.Set("products_list_version", current + 1, TimeSpan.FromDays(1));

        return Unit.Value;
    }
}


