using System.ComponentModel.DataAnnotations;

namespace StockFlow.Models;

public sealed class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required, StringLength(30)]
    public string Size { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Color { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Sku { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Barcode { get; set; }

    [Range(0, 99999999)]
    public decimal CostPrice { get; set; }

    [Range(0.01, 99999999)]
    public decimal SellingPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantityOnHand { get; set; }

    [Range(0, int.MaxValue)]
    public int LowStockThreshold { get; set; } = 5;

    public bool IsActive { get; set; } = true;

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    public ICollection<StockTransaction> StockTransactions { get; set; } = [];
    public ICollection<SaleItem> SaleItems { get; set; } = [];
}
