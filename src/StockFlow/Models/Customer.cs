using System.ComponentModel.DataAnnotations;

namespace StockFlow.Models;

public sealed class Customer
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(160)]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Sale> Sales { get; set; } = [];
}
