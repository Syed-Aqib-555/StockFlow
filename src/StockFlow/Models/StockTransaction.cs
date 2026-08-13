using System.ComponentModel.DataAnnotations;
using StockFlow.Data;

namespace StockFlow.Models;

public sealed class StockTransaction
{
    public int Id { get; set; }
    public int VariantId { get; set; }
    public ProductVariant Variant { get; set; } = null!;
    public StockTransactionType Type { get; set; }
    public int QuantityChange { get; set; }
    public int QuantityAfter { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Required, StringLength(300)]
    public string Notes { get; set; } = string.Empty;

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
