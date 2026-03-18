using System;
using System.Collections.Generic;
using ECommerce.Application.DTOs.Generic;
using ECommerce.Application.DTOs.Order;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries.All;

public sealed record GetOrdersQuery(
    int UserId,
    int Page,
    int PageSize,
    string? Search,
    string? Status,
    DateTime? StartDate,
    DateTime? EndDate,
    string? SortBy,
    bool Desc
) : IRequest<PagedResult<OrderResponseDto>>;

