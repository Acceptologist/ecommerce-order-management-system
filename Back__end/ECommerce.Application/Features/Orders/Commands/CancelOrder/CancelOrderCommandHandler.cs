using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.Notification;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public CancelOrderCommandHandler(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
    }

    public async Task<Unit> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<Order>();
        var order = await repo.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Unit.Value;
        }

        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed || order.Status == OrderStatus.Shipped)
        {
            throw new InvalidOperationException($"Order cannot be cancelled (current status: {order.Status}).");
        }

        // Load items to restore stock
        var itemsRepo = _uow.Repository<OrderItem>();
        var items = await itemsRepo.Query()
            .Where(i => i.OrderId == order.Id)
            .ToListAsync(cancellationToken);

        var productRepo = _uow.Repository<Product>();
        foreach (var item in items)
        {
            var product = await productRepo.GetByIdAsync(item.ProductId, cancellationToken);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                productRepo.Update(product);

                // Notify frontend about stock update
                await _notificationService.BroadcastStockUpdateAsync(product.Id, product.StockQuantity, cancellationToken);
            }
        }

        order.Status = OrderStatus.Cancelled;
        repo.Update(order);
        await _uow.SaveChangesAsync(cancellationToken);

        // notify the order owner
        var notification = new Notification
        {
            UserId = order.UserId,
            OrderId = order.Id,
            Type = "Warning",
            Message = $"Order #{order.Id} has been cancelled.",
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
