using ECommerce.Application.DTOs.Order;
using MediatR;

namespace ECommerce.Application.Features.Orders.Commands.Create;

public record CreateOrderCommand(int UserId, CreateOrderRequest Request) : IRequest<OrderResponseDto>;

