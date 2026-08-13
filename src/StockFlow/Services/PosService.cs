using System.Data;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.Models;

namespace StockFlow.Services;

public sealed class PosService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<SaleResult> CompleteSaleAsync(CreateSaleRequest request, string? userId)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Add at least one item before checkout.");
        }

        if (request.Items.Any(x => x.Quantity <= 0))
        {
            throw new InvalidOperationException("Every cart quantity must be at least one.");
        }

        if (request.Discount < 0)
        {
            throw new InvalidOperationException("Discount cannot be negative.");
        }

        var combinedItems = request.Items
            .GroupBy(x => x.VariantId)
            .Select(x => new CartLineRequest(x.Key, x.Sum(y => y.Quantity)))
            .ToArray();

        await using var db = await contextFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var ids = combinedItems.Select(x => x.VariantId).ToArray();
        var variants = await db.ProductVariants
            .Include(x => x.Product)
            .Where(x => ids.Contains(x.Id) && x.IsActive && x.Product.IsActive)
            .ToDictionaryAsync(x => x.Id);

        if (variants.Count != ids.Length)
        {
            throw new InvalidOperationException("One or more cart items are unavailable.");
        }

        foreach (var line in combinedItems)
        {
            var variant = variants[line.VariantId];
            if (variant.QuantityOnHand < line.Quantity)
            {
                throw new InvalidOperationException($"{variant.Product.Name} ({variant.Size}, {variant.Color}) has only {variant.QuantityOnHand} in stock.");
            }
        }

        var subtotal = combinedItems.Sum(x => variants[x.VariantId].SellingPrice * x.Quantity);
        if (request.Discount > subtotal)
        {
            throw new InvalidOperationException("Discount cannot be greater than the subtotal.");
        }

        var sale = new Sale
        {
            SaleNumber = $"SF-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            CustomerId = request.CustomerId,
            Date = DateTime.UtcNow,
            PaymentMethod = request.PaymentMethod,
            Subtotal = subtotal,
            Discount = request.Discount,
            Total = subtotal - request.Discount,
            Status = SaleStatus.Completed,
            CreatedByUserId = userId
        };

        db.Sales.Add(sale);
        foreach (var line in combinedItems)
        {
            var variant = variants[line.VariantId];
            variant.QuantityOnHand -= line.Quantity;
            sale.Items.Add(new SaleItem
            {
                VariantId = variant.Id,
                Quantity = line.Quantity,
                UnitPrice = variant.SellingPrice,
                UnitCost = variant.CostPrice
            });
            db.StockTransactions.Add(new StockTransaction
            {
                VariantId = variant.Id,
                Type = StockTransactionType.Sale,
                QuantityChange = -line.Quantity,
                QuantityAfter = variant.QuantityOnHand,
                Date = DateTime.UtcNow,
                Notes = $"POS sale {sale.SaleNumber}",
                UserId = userId
            });
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return new SaleResult(sale.Id, sale.SaleNumber, sale.Total);
    }

    public async Task VoidSaleAsync(int saleId, string reason, string? userId)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("A reason is required to void a sale.");
        }

        await using var db = await contextFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var sale = await db.Sales.Include(x => x.Items).ThenInclude(x => x.Variant)
            .SingleOrDefaultAsync(x => x.Id == saleId)
            ?? throw new InvalidOperationException("Sale not found.");

        if (sale.Status == SaleStatus.Cancelled)
        {
            throw new InvalidOperationException("This sale is already voided.");
        }

        sale.Status = SaleStatus.Cancelled;
        sale.CancelledAt = DateTime.UtcNow;
        sale.CancellationReason = reason.Trim();
        foreach (var item in sale.Items)
        {
            item.Variant.QuantityOnHand += item.Quantity;
            db.StockTransactions.Add(new StockTransaction
            {
                VariantId = item.VariantId,
                Type = StockTransactionType.SaleVoided,
                QuantityChange = item.Quantity,
                QuantityAfter = item.Variant.QuantityOnHand,
                Date = DateTime.UtcNow,
                Notes = $"Voided sale {sale.SaleNumber}: {sale.CancellationReason}",
                UserId = userId
            });
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
