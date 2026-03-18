using System.ComponentModel.DataAnnotations;

namespace ECommerce.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>Optional per-product discount rate (0–100). Applied on top of order-level discount.</summary>
    public decimal DiscountRate { get; set; }

    /// <summary>Row version for optimistic concurrency (prevents overselling under concurrent orders).</summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    /// <summary>Soft delete: when true, product is hidden from listings.</summary>
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
