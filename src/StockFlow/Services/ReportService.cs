using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.Models;

namespace StockFlow.Services;

public sealed class ReportService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<ReportSnapshot> GetAsync(DateOnly from, DateOnly to)
    {
        if (to < from)
        {
            throw new InvalidOperationException("The end date must be on or after the start date.");
        }

        var start = from.ToDateTime(TimeOnly.MinValue);
        var end = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
        await using var db = await contextFactory.CreateDbContextAsync();
        var sales = await db.Sales
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.Status == SaleStatus.Completed && x.Date >= start && x.Date < end)
            .ToListAsync();

        var rows = sales
            .GroupBy(x => DateOnly.FromDateTime(x.Date))
            .OrderByDescending(x => x.Key)
            .Select(group =>
            {
                var revenue = group.Sum(x => x.Total);
                var cost = group.SelectMany(x => x.Items).Sum(x => x.UnitCost * x.Quantity);
                return new ReportRow(group.Key, group.Count(), revenue, cost, revenue - cost);
            })
            .ToList();

        var totalRevenue = sales.Sum(x => x.Total);
        var totalCost = sales.SelectMany(x => x.Items).Sum(x => x.UnitCost * x.Quantity);
        return new ReportSnapshot(rows, totalRevenue, totalCost, totalRevenue - totalCost,
            sales.SelectMany(x => x.Items).Sum(x => x.Quantity), sales.Count);
    }
}
