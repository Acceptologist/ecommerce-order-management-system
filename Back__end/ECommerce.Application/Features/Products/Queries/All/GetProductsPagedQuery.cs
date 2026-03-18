using ECommerce.Application.DTOs.Generic;
using ECommerce.Application.DTOs.Product;
using MediatR;

namespace ECommerce.Application.Features.Products.Queries.All;

public record GetProductsPagedQuery(
    int Page,
    int PageSize,
    string? Search,
    int? CategoryId,
    string? SortBy,
    bool Desc
) : IRequest<PagedResult<ProductListItemDto>>;

