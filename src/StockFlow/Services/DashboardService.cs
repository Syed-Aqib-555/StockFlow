using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.Models;

namespace StockFlow.Services;

public sealed class DashboardService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<DashboardSnapshot> GetSnapshotAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var sevenDaysAgo = today.AddDays(-6);
        var completedSales = db.Sales.Where(x => x.Status == SaleStatus.Completed);

        var todaySales = await completedSales.Where(x => x.Date >= today && x.Date < tomorrow).SumAsync(x => (decimal?)x.Total) ?? 0;
        var monthSales = await completedSales.Where(x => x.Date >= monthStart).SumAsync(x => (decimal?)x.Total) ?? 0;
        var ordersToday = await completedSales.CountAsync(x => x.Date >= today && x.Date < tomorrow);
        var productCount = await db.Products.CountAsync(x => x.IsActive);
        var variantCount = await db.ProductVariants.CountAsync(x => x.IsActive && x.Product.IsActive);
        var lowStockCount = await db.ProductVariants.CountAsync(x => x.IsActive && x.QuantityOnHand <= x.LowStockThreshold);

        var monthItems = await db.SaleItems
            .Where(x => x.Sale.Status == SaleStatus.Completed && x.Sale.Date >= monthStart)
            .Select(x => new { x.Quantity, x.UnitPrice, x.UnitCost, x.Sale.Discount, x.Sale.Subtotal })
            .ToListAsync();
        var grossProfit = monthItems.Sum(x => (x.UnitPrice - x.UnitCost) * x.Quantity);
        var monthDiscounts = await completedSales.Where(x => x.Date >= monthStart).SumAsync(x => (decimal?)x.Discount) ?? 0;

        var topProductRows = await db.SaleItems
            .Where(x => x.Sale.Status == SaleStatus.Completed && x.Sale.Date >= monthStart)
            .GroupBy(x => new { x.Variant.ProductId, x.Variant.Product.Name })
            .Select(x => new { x.Key.Name, Quantity = x.Sum(y => y.Quantity), Revenue = x.Sum(y => y.Quantity * y.UnitPrice) })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToListAsync();
        var topProducts = topProductRows
            .Select(x => new TopProduct(x.Name, x.Quantity, x.Revenue))
            .ToList();

        var dailyTotals = await completedSales
            .Where(x => x.Date >= sevenDaysAgo)
            .GroupBy(x => x.Date.Date)
            .Select(x => new { Date = x.Key, Total = x.Sum(y => y.Total) })
            .ToListAsync();
        var trend = Enumerable.Range(0, 7)
            .Select(offset => DateOnly.FromDateTime(sevenDaysAgo.AddDays(offset)))
            .Select(date => new DailySalesPoint(date, dailyTotals.FirstOrDefault(x => DateOnly.FromDateTime(x.Date) == date)?.Total ?? 0))
            .ToList();

        return new DashboardSnapshot(todaySales, monthSales, grossProfit - monthDiscounts, productCount, variantCount, lowStockCount, ordersToday, topProducts, trend);
    }
}
