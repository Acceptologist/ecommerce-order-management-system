using ECommerce.Domain.Enums;
using MediatR;

namespace ECommerce.Application.Features.Orders.Commands.UpdateStatus;

public sealed record UpdateOrderStatusCommand(int OrderId, OrderStatus NewStatus) : IRequest;

