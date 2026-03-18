using ECommerce.Application.DTOs.Notification;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using MediatR;

namespace ECommerce.Application.Features.Orders.Commands.UpdateStatus;

public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public UpdateOrderStatusCommandHandler(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
    }

    public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<Order>();
        var order = await repo.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Unit.Value;
        }

        if (order.Status == request.NewStatus)
        {
            return Unit.Value;
        }

        order.Status = request.NewStatus;
        repo.Update(order);
        await _uow.SaveChangesAsync(cancellationToken);

        string userFriendlyMessage = request.NewStatus switch
        {
            OrderStatus.Processing => $"Great news! We've started processing your order #{order.Id}.",
            OrderStatus.Shipped => $"Yay! Your order #{order.Id} is on its way to you.",
            OrderStatus.Completed => $"Your order #{order.Id} has been delivered. Enjoy!",
            OrderStatus.Cancelled => $"Your order #{order.Id} has been cancelled.",
            _ => $"Order #{order.Id} status updated to {order.Status}."
        };

        // notify owner about status change
        var notification = new Notification
        {
            UserId = order.UserId,
            OrderId = order.Id,
            Type = "Info",
            Message = userFriendlyMessage,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _uow.Repository<Notification>().AddAsync(notification, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _notificationService.SendToUserAsync(order.UserId.ToString(), new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId ?? 0,
            OrderId = order.Id,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        }, cancellationToken);

        return Unit.Value;
    }
}

