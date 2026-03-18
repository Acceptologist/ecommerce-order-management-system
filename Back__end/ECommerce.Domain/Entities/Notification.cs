namespace ECommerce.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? OrderId { get; set; }
    public string Type { get; set; } = "Info";
    public string Message { get; set; } = default!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
