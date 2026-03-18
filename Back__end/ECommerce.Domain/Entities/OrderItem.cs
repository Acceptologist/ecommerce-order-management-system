namespace ECommerce.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = default!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public decimal DiscountAtPurchase { get; set; }
}
