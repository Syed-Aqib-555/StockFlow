using System.ComponentModel.DataAnnotations;

namespace StockFlow.Models;

public sealed class Sale
{
    public int Id { get; set; }

    [Required, StringLength(40)]
    public string SaleNumber { get; set; } = string.Empty;

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Completed;
    public DateTime? CancelledAt { get; set; }

    [StringLength(300)]
    public string? CancellationReason { get; set; }

    public string? CreatedByUserId { get; set; }
    public ICollection<SaleItem> Items { get; set; } = [];

    public decimal Profit => Items.Sum(x => (x.UnitPrice - x.UnitCost) * x.Quantity) - Discount;
}
