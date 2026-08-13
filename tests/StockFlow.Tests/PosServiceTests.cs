using Microsoft.EntityFrameworkCore;
using StockFlow.Models;
using StockFlow.Services;

namespace StockFlow.Tests;

public sealed class PosServiceTests
{
    [Fact]
    public async Task CompleteSaleAsync_SavesSaleItems_ReducesStock_AndPreservesCost()
    {
        var factory = await TestDatabase.CreateAsync(initialStock: 10);
        var service = new PosService(factory);
        var request = new CreateSaleRequest([new CartLineRequest(1, 2)], 1, PaymentMethod.Cash, 2m);

        var result = await service.CompleteSaleAsync(request, "cashier-1");

        await using var db = await factory.CreateDbContextAsync();
        var sale = await db.Sales.Include(x => x.Items).SingleAsync();
        Assert.Equal(result.SaleId, sale.Id);
        Assert.Equal(42m, sale.Total);
        Assert.Equal(8m, sale.Items.Single().UnitCost);
        Assert.Equal(8, (await db.ProductVariants.SingleAsync()).QuantityOnHand);
        var movement = await db.StockTransactions.SingleAsync();
        Assert.Equal(-2, movement.QuantityChange);
        Assert.Equal(8, movement.QuantityAfter);
        Assert.Equal(StockTransactionType.Sale, movement.Type);
    }

    [Fact]
    public async Task CompleteSaleAsync_RejectsOverselling_WithoutPartialWrites()
    {
        var factory = await TestDatabase.CreateAsync(initialStock: 1);
        var service = new PosService(factory);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CompleteSaleAsync(new CreateSaleRequest([new CartLineRequest(1, 2)], null, PaymentMethod.Card, 0), "cashier-1"));

        Assert.Contains("only 1 in stock", exception.Message);
        await using var db = await factory.CreateDbContextAsync();
        Assert.Empty(await db.Sales.ToListAsync());
        Assert.Empty(await db.StockTransactions.ToListAsync());
        Assert.Equal(1, (await db.ProductVariants.SingleAsync()).QuantityOnHand);
    }

    [Fact]
    public async Task VoidSaleAsync_MarksSaleCancelled_AndRestoresStock()
    {
        var factory = await TestDatabase.CreateAsync(initialStock: 6);
        var service = new PosService(factory);
        var completed = await service.CompleteSaleAsync(
            new CreateSaleRequest([new CartLineRequest(1, 3)], null, PaymentMethod.Cash, 0), "cashier-1");

        await service.VoidSaleAsync(completed.SaleId, "Customer returned before leaving", "admin-1");

        await using var db = await factory.CreateDbContextAsync();
        var sale = await db.Sales.SingleAsync();
        Assert.Equal(SaleStatus.Cancelled, sale.Status);
        Assert.Equal(6, (await db.ProductVariants.SingleAsync()).QuantityOnHand);
        var movements = await db.StockTransactions.OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(2, movements.Count);
        Assert.Equal(3, movements[1].QuantityChange);
        Assert.Equal(StockTransactionType.SaleVoided, movements[1].Type);
    }

    [Fact]
    public async Task CompleteSaleAsync_RejectsDiscountAboveSubtotal()
    {
        var factory = await TestDatabase.CreateAsync();
        var service = new PosService(factory);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CompleteSaleAsync(new CreateSaleRequest([new CartLineRequest(1, 1)], null, PaymentMethod.Cash, 25m), "cashier-1"));
    }
}
