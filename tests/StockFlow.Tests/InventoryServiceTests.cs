using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StockFlow.Data;
using StockFlow.Models;
using StockFlow.Services;

namespace StockFlow.Tests;

public sealed class InventoryServiceTests
{
    [Fact]
    public async Task ReceiveAsync_IncreasesStock_AndCreatesAuditEntry()
    {
        var factory = await TestDatabase.CreateAsync(initialStock: 4);
        var service = new InventoryService(factory);

        await service.ReceiveAsync(1, 6, "Delivery note DN-100", "admin-1");

        await using var db = await factory.CreateDbContextAsync();
        var variant = await db.ProductVariants.SingleAsync();
        var transaction = await db.StockTransactions.SingleAsync();
        Assert.Equal(10, variant.QuantityOnHand);
        Assert.Equal(6, transaction.QuantityChange);
        Assert.Equal(10, transaction.QuantityAfter);
        Assert.Equal(StockTransactionType.Received, transaction.Type);
    }

    [Fact]
    public async Task AdjustAsync_RejectsAChangeThatWouldCreateNegativeStock()
    {
        var factory = await TestDatabase.CreateAsync(initialStock: 3);
        var service = new InventoryService(factory);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AdjustAsync(1, -4, StockTransactionType.Damaged, "Damaged box", "admin-1"));

        Assert.Contains("cannot be negative", exception.Message);
        await using var db = await factory.CreateDbContextAsync();
        Assert.Equal(3, (await db.ProductVariants.SingleAsync()).QuantityOnHand);
        Assert.Empty(await db.StockTransactions.ToListAsync());
    }

    [Fact]
    public async Task AdjustAsync_RequiresAnAuditNote()
    {
        var factory = await TestDatabase.CreateAsync(initialStock: 3);
        var service = new InventoryService(factory);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AdjustAsync(1, 1, StockTransactionType.Return, " ", "admin-1"));
    }
}

internal sealed class TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
    : IDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext() => new(options);
    public Task<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(CreateDbContext());
}

internal static class TestDatabase
{
    public static async Task<TestDbContextFactory> CreateAsync(int initialStock = 10)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"stockflow-tests-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var factory = new TestDbContextFactory(options);
        await using var db = await factory.CreateDbContextAsync();
        var category = new Category { Id = 1, Name = "Test category" };
        var product = new Product { Id = 1, Name = "Test T-Shirt", Category = category };
        product.Variants.Add(new ProductVariant
        {
            Id = 1,
            Size = "M",
            Color = "Black",
            Sku = "TEST-BLK-M",
            CostPrice = 8m,
            SellingPrice = 22m,
            QuantityOnHand = initialStock,
            LowStockThreshold = 3
        });
        db.Products.Add(product);
        db.Customers.Add(new Customer { Id = 1, Name = "Test Customer", Phone = "555-0100" });
        await db.SaveChangesAsync();
        return factory;
    }
}
