using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Application.Features.Products.Commands.Create;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _memoryCache;

    public CreateProductCommandHandler(IUnitOfWork uow, IMemoryCache memoryCache)
    {
        _uow = uow;
        _memoryCache = memoryCache;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var entity = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl!.Trim(),
            DiscountRate = dto.DiscountRate
        };

        await _uow.Repository<Product>().AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var current = (int?)(_memoryCache.Get("products_list_version") ?? 0) ?? 0;
        _memoryCache.Set("products_list_version", current + 1, TimeSpan.FromDays(1));

        return entity.Id;
    }
}

