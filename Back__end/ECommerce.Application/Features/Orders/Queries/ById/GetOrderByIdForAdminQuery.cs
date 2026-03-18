using ECommerce.Application.DTOs.Order;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries.ById;

public sealed record GetOrderByIdForAdminQuery(int OrderId) : IRequest<OrderResponseDto?>;
