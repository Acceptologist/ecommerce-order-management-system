using ECommerce.Application.DTOs.Notification;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    // Broadcast to all connected clients (admin announcements etc.)
    public Task SendAsync(NotificationDto notification, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.All.SendAsync("ReceiveNotification", notification, cancellationToken);

    // Send to a specific user only.
    // NOTE: do not send to both User() and Group() to avoid duplicate notifications.
    public Task SendToUserAsync(string userId, NotificationDto notification, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification, cancellationToken);

    public Task BroadcastStockUpdateAsync(int productId, int newStockQuantity, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync("StockUpdated", new { productId, newStockQuantity }, cancellationToken);
    }
}

