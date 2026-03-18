namespace ECommerce.Application.DTOs.Product;

public class ReadProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public decimal DiscountRate { get; set; }
}

