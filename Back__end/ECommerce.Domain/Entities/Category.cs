namespace ECommerce.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    /// <summary>Soft delete: when true, category is hidden from listings.</summary>
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
