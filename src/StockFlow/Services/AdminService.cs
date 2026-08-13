using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.Models;

namespace StockFlow.Services;

public sealed class AdminService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    DashboardService dashboardService)
{
    public async Task<AdminSnapshot> GetSnapshotAsync()
    {
        var operations = await dashboardService.GetSnapshotAsync();
        await using var db = await contextFactory.CreateDbContextAsync();

        var inventoryValue = await db.ProductVariants
            .Where(x => x.IsActive && x.Product.IsActive)
            .SumAsync(x => (decimal?)(x.QuantityOnHand * x.CostPrice)) ?? 0;
        var outOfStockCount = await db.ProductVariants
            .CountAsync(x => x.IsActive && x.Product.IsActive && x.QuantityOnHand == 0);
        var customerCount = await db.Customers.CountAsync();
        var teamMemberCount = await db.Users.CountAsync();
        var adminRoleId = await db.Roles
            .Where(x => x.NormalizedName == "ADMIN")
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
        var adminCount = adminRoleId is null
            ? 0
            : await db.UserRoles.CountAsync(x => x.RoleId == adminRoleId);

        var recentSales = await db.Sales
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .OrderByDescending(x => x.Date)
            .Take(6)
            .Select(x => new AdminRecentSale(
                x.Id,
                x.SaleNumber,
                x.Date,
                x.Customer == null ? "Walk-in customer" : x.Customer.Name,
                x.Total,
                x.PaymentMethod,
                x.Status,
                x.Items.Sum(item => item.Quantity)))
            .ToListAsync();

        var lowStockItems = await db.ProductVariants
            .AsNoTracking()
            .Where(x => x.IsActive && x.Product.IsActive && x.QuantityOnHand <= x.LowStockThreshold)
            .OrderBy(x => x.QuantityOnHand)
            .ThenBy(x => x.Product.Name)
            .Take(6)
            .Select(x => new AdminLowStockItem(
                x.Id,
                x.Product.Name,
                x.Sku,
                x.Size + " / " + x.Color,
                x.QuantityOnHand,
                x.LowStockThreshold))
            .ToListAsync();

        var recentStockEvents = await db.StockTransactions
            .AsNoTracking()
            .OrderByDescending(x => x.Date)
            .Take(5)
            .Select(x => new AdminStockEvent(
                x.Variant.Product.Name,
                x.Variant.Sku,
                x.Type,
                x.QuantityChange,
                x.QuantityAfter,
                x.Date))
            .ToListAsync();

        return new AdminSnapshot(
            operations,
            inventoryValue,
            customerCount,
            outOfStockCount,
            teamMemberCount,
            adminCount,
            recentSales,
            lowStockItems,
            recentStockEvents);
    }
}
