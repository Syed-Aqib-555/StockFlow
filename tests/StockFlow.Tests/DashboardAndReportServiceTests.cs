using Microsoft.EntityFrameworkCore;
using StockFlow.Models;
using StockFlow.Services;

namespace StockFlow.Tests;

public sealed class DashboardAndReportServiceTests
{
    [Fact]
    public async Task DashboardSnapshot_ReflectsCompletedSalesAndLowStock()
    {
        var factory = await TestDatabase.CreateAsync(initialStock: 2);
        var pos = new PosService(factory);
        await pos.CompleteSaleAsync(
            new CreateSaleRequest([new CartLineRequest(1, 1)], 1, PaymentMethod.Cash, 0),
            "cashier-1");

        var snapshot = await new DashboardService(factory).GetSnapshotAsync();

        Assert.Equal(22m, snapshot.TodaySales);
        Assert.Equal(1, snapshot.OrdersToday);
        Assert.Equal(14m, snapshot.MonthProfit);
        Assert.Equal(1, snapshot.LowStockCount);
        Assert.Single(snapshot.TopProducts);
    }

    [Fact]
    public async Task ReportSnapshot_UsesHistoricalUnitCostForProfit()
    {
        var factory = await TestDatabase.CreateAsync(initialStock: 5);
        var pos = new PosService(factory);
        await pos.CompleteSaleAsync(
            new CreateSaleRequest([new CartLineRequest(1, 2)], null, PaymentMethod.Card, 4m),
            "cashier-1");

        await using (var db = await factory.CreateDbContextAsync())
        {
            var variant = await db.ProductVariants.SingleAsync();
            variant.CostPrice = 100m;
            await db.SaveChangesAsync();
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var report = await new ReportService(factory).GetAsync(today, today);

        Assert.Equal(40m, report.Revenue);
        Assert.Equal(16m, report.Cost);
        Assert.Equal(24m, report.Profit);
        Assert.Equal(2, report.UnitsSold);
    }
}
