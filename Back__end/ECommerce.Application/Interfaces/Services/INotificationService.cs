using ECommerce.Application.DTOs.Notification;

namespace ECommerce.Application.Interfaces.Services;

public interface INotificationService
{
    Task SendAsync(NotificationDto notification, CancellationToken cancellationToken = default);
    Task SendToUserAsync(string userId, NotificationDto notification, CancellationToken cancellationToken = default);
    /// <summary>Broadcast stock update to all clients for real-time UI (e.g. product list).</summary>
    Task BroadcastStockUpdateAsync(int productId, int newStockQuantity, CancellationToken cancellationToken = default);
}

