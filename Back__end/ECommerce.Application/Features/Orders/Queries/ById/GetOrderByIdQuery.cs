using ECommerce.Application.DTOs.Order;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries.ById;

public sealed record GetOrderByIdQuery(int UserId, int OrderId) : IRequest<OrderResponseDto?>;

