using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountApplied { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Shipping address (owned entity)
    public string? ShippingStreet  { get; set; }
    public string? ShippingCity    { get; set; }
    public string? ShippingCountry { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
