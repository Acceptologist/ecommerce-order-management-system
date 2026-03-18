namespace ECommerce.Application.Options;

/// <summary>
/// Configurable order discount rules (e.g. 10% over threshold).
/// </summary>
public class OrderDiscountOptions
{
    public const string SectionName = "Order";

    /// <summary>Subtotal above this amount (inclusive) gets the discount.</summary>
    public decimal DiscountThreshold { get; set; } = 10000m;

    /// <summary>Discount percentage (e.g. 10 for 10%).</summary>
    public decimal DiscountPercent { get; set; } = 10m;

    /// <summary>Human-readable description for UI, e.g. "10% over $100".</summary>
    public string DiscountDescription => $"{DiscountPercent}% over ${DiscountThreshold:N0}";
}
