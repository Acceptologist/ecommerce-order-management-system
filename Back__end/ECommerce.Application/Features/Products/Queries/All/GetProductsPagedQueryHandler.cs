using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Application.DTOs.Generic;
using ECommerce.Application.DTOs.Product;
using ECommerce.Domain.Entities;
using MediatR;

// NOTE: removed EntityFrameworkCore using to keep application layer infrastructure-agnostic. Async helpers provided by repository.

namespace ECommerce.Application.Features.Products.Queries.All;

using Microsoft.Extensions.Caching.Memory;

public class GetProductsPagedQueryHandler : IRequestHandler<GetProductsPagedQuery, PagedResult<ProductListItemDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _memoryCache;

    public GetProductsPagedQueryHandler(IUnitOfWork uow, IMemoryCache memoryCache)
    {
        _uow = uow;
        _memoryCache = memoryCache;
    }

    public async Task<PagedResult<ProductListItemDto>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        // Cache key includes a version so we can invalidate all product lists when a product is updated/created/deleted
        var version = _memoryCache.GetOrCreate("products_list_version", e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            return 0;
        });
        var cacheKey = $"products-v{version}-{request.Page}-{request.PageSize}-{request.Search}-{request.CategoryId}-{request.SortBy}-{request.Desc}";
        if (_memoryCache.TryGetValue(cacheKey, out PagedResult<ProductListItemDto>? cached) && cached is not null)
        {
            return cached;
        }

        // start with base query (exclude soft-deleted), projection will handle category name via join
        var query = _uow.Repository<Product>().Query().Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        query = request.SortBy switch
        {
            "price"  => request.Desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "name"   => request.Desc ? query.OrderByDescending(p => p.Name)  : query.OrderBy(p => p.Name),
            "stock"  => request.Desc ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
            _        => query.OrderBy(p => p.Id)
        };

        // use repository async helpers instead of EF Core extensions
        var total = await _uow.Repository<Product>().CountAsync(query, cancellationToken);

        var pagedProducts = await _uow.Repository<Product>().ToListAsync(
            query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize), cancellationToken);

        var items = pagedProducts.Select(p => new ProductListItemDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            CategoryName = p.Category?.Name ?? string.Empty,
            ImageUrl = p.ImageUrl,
            DiscountRate = p.DiscountRate
        }).ToList();

        var result = new PagedResult<ProductListItemDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = total,
            Items = items
        };

        _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(2));

        return result;
    }
}

