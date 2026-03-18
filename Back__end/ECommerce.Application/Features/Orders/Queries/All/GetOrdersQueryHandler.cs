using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.Generic;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Orders.Queries.All;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderResponseDto>>
{
    private readonly IUnitOfWork _uow;

    public GetOrdersQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResult<OrderResponseDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _uow.Repository<Order>().Query().Where(o => o.UserId == request.UserId);

        // Status Filter
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<OrderStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(o => o.Status == statusEnum);
            }
        }

        // Date Range Filter
        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value.Date;
            query = query.Where(o => o.OrderDate >= start);
        }
        if (request.EndDate.HasValue)
        {
            var end = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(o => o.OrderDate <= end);
        }

        // Search by Order ID
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchStr = request.Search.Trim();
            if (int.TryParse(searchStr, out var orderId))
            {
                query = query.Where(o => o.Id == orderId);
            }
            else
            {
                // For a specific user, searching by customer name doesn't make sense, so if it's not a number we return empty
                query = query.Where(o => false);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            query = request.SortBy.ToLower() switch
            {
                "id" => request.Desc ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id),
                "date" => request.Desc ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate),
                "total" => request.Desc ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
                "items" => request.Desc ? query.OrderByDescending(o => o.Items.Sum(i => i.Quantity)) : query.OrderBy(o => o.Items.Sum(i => i.Quantity)),
                "status" => request.Desc ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
                _ => request.Desc ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate)
            };
        }
        else
        {
            query = query.OrderByDescending(o => o.OrderDate);
        }

        // Pagination
        var skip = (request.Page - 1) * request.PageSize;
        query = query.Skip(skip).Take(request.PageSize);

        var inc = query.Include(o => o.Items);
        query = inc.ThenInclude(i => i.Product);

        var orders = await _uow.Repository<Order>().ToListAsync(query, cancellationToken);

        var items = orders
            .Select(o => new OrderResponseDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                DiscountApplied = o.DiscountApplied,
                ItemsCount = o.Items.Count,
                ItemsQuantity = o.Items.Sum(i => i.Quantity),
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                Address = o.ShippingStreet == null ? null : new ShippingAddressDto
                {
                    Street = o.ShippingStreet!,
                    City = o.ShippingCity!,
                    Country = o.ShippingCountry!
                },
                Items = o.Items.Select(i => new OrderItemResponseDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    PriceAtPurchase = i.PriceAtPurchase
                }).ToList()
            })
            .ToList();

        return new PagedResult<OrderResponseDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            Items = items
        };
    }
}

