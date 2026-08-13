using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.Models;

namespace StockFlow.Services;

public sealed class InventoryService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public Task ReceiveAsync(int variantId, int quantity, string notes, string? userId) =>
        ChangeStockAsync(variantId, Math.Abs(quantity), StockTransactionType.Received, notes, userId);

    public Task AdjustAsync(int variantId, int quantityChange, StockTransactionType type, string notes, string? userId) =>
        ChangeStockAsync(variantId, quantityChange, type, notes, userId);

    private async Task ChangeStockAsync(
        int variantId,
        int quantityChange,
        StockTransactionType type,
        string notes,
        string? userId)
    {
        if (quantityChange == 0)
        {
            throw new InvalidOperationException("Stock change cannot be zero.");
        }

        if (string.IsNullOrWhiteSpace(notes))
        {
            throw new InvalidOperationException("A stock note is required for the audit trail.");
        }

        await using var db = await contextFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var variant = await db.ProductVariants.SingleOrDefaultAsync(x => x.Id == variantId)
            ?? throw new InvalidOperationException("The selected product variant no longer exists.");

        var newQuantity = variant.QuantityOnHand + quantityChange;
        if (newQuantity < 0)
        {
            throw new InvalidOperationException($"Only {variant.QuantityOnHand} units are available. Stock cannot be negative.");
        }

        variant.QuantityOnHand = newQuantity;
        db.StockTransactions.Add(new StockTransaction
        {
            VariantId = variantId,
            Type = type,
            QuantityChange = quantityChange,
            QuantityAfter = newQuantity,
            Date = DateTime.UtcNow,
            Notes = notes.Trim(),
            UserId = userId
        });

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
