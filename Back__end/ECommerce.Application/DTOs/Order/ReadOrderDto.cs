namespace ECommerce.Application.DTOs.Order;

public class ReadOrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public decimal DiscountAtPurchase { get; set; }
}

public class ReadOrderDto
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountApplied { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = default!;
    public List<ReadOrderItemDto> Items { get; set; } = new();
}

