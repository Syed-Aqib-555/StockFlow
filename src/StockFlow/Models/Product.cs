using System.ComponentModel.DataAnnotations;

namespace StockFlow.Models;

public sealed class Product
{
    public int Id { get; set; }

    [Required, StringLength(140)]
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    [StringLength(1200)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ProductVariant> Variants { get; set; } = [];
}
