using System.ComponentModel.DataAnnotations;

namespace StockFlow.Models;

public sealed class Category
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = [];
}
