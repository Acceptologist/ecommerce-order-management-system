namespace ECommerce.Application.DTOs.Notification;

public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? OrderId { get; set; }
    public string Message { get; set; } = default!;
    public string Type { get; set; } = "Info";   // "Success" | "Error" | "Warning" | "Info"
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

