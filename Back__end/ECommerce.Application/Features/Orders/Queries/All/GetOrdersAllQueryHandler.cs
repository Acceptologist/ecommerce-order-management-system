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

public sealed class GetOrdersAllQueryHandler : IRequestHandler<GetOrdersAllQuery, PagedResult<OrderResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepo;

    public GetOrdersAllQueryHandler(IUnitOfWork uow, IUserRepository userRepo)
    {
        _uow = uow;
        _userRepo = userRepo;
    }

    public async Task<PagedResult<OrderResponseDto>> Handle(GetOrdersAllQuery request, CancellationToken cancellationToken)
    {
        var query = _uow.Repository<Order>().Query();
        
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
                // If it's a number, it could be an order ID
                query = query.Where(o => o.Id == orderId);
            }
            // Note: Searching by Customer Name is tricky here because User is not in the same DbContext/table directly if we rely on IUserRepository.
            // For a robust search by customer name, we would need to fetch matching user IDs first, or join. 
            // We will fetch matching user IDs from userRepo if search is not a number.
            else
            {
                var matchingUserIds = await _userRepo.SearchUserIdsByNameAsync(searchStr, cancellationToken);
                if (matchingUserIds != null && matchingUserIds.Any())
                {
                    query = query.Where(o => matchingUserIds.Contains(o.UserId));
                }
                else
                {
                    // No users matched, return empty
                    query = query.Where(o => false);
                }
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
        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var userNames = await _userRepo.GetUserNamesByIdsAsync(userIds, cancellationToken);

        var items = orders
            .Select(o => new OrderResponseDto
            {
                Id = o.Id,
                UserId = o.UserId,
                UserName = userNames.ContainsKey(o.UserId) ? userNames[o.UserId] : null,
                Subtotal = o.Items.Sum(i => i.PriceAtPurchase * i.Quantity),
                TotalAmount = o.TotalAmount,
                DiscountApplied = o.DiscountApplied,
                DiscountDescription = o.DiscountApplied > 0 ? "Order discount" : null,
                ItemsCount = o.Items.Count,
                ItemsQuantity = o.Items.Sum(i => i.Quantity),
                OrderDate = o.OrderDate,
                EstimatedDeliveryDate = o.OrderDate.AddDays(7),
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
