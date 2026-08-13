namespace StockFlow.Models;

public sealed class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int VariantId { get; set; }
    public ProductVariant Variant { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}
