using StockFlow.Models;

namespace StockFlow.Services;

public sealed record CartLineRequest(int VariantId, int Quantity);

public sealed record CreateSaleRequest(
    IReadOnlyCollection<CartLineRequest> Items,
    int? CustomerId,
    PaymentMethod PaymentMethod,
    decimal Discount);

public sealed record SaleResult(int SaleId, string SaleNumber, decimal Total);

public sealed record TopProduct(string Name, int Quantity, decimal Revenue);

public sealed record DailySalesPoint(DateOnly Date, decimal Total);

public sealed record DashboardSnapshot(
    decimal TodaySales,
    decimal MonthSales,
    decimal MonthProfit,
    int ProductCount,
    int VariantCount,
    int LowStockCount,
    int OrdersToday,
    IReadOnlyList<TopProduct> TopProducts,
    IReadOnlyList<DailySalesPoint> SalesTrend);

public sealed record AdminRecentSale(
    int Id,
    string SaleNumber,
    DateTime Date,
    string Customer,
    decimal Total,
    PaymentMethod PaymentMethod,
    SaleStatus Status,
    int ItemCount);

public sealed record AdminLowStockItem(
    int VariantId,
    string ProductName,
    string Sku,
    string Option,
    int Quantity,
    int Threshold);

public sealed record AdminStockEvent(
    string ProductName,
    string Sku,
    StockTransactionType Type,
    int QuantityChange,
    int QuantityAfter,
    DateTime Date);

public sealed record AdminSnapshot(
    DashboardSnapshot Operations,
    decimal InventoryValue,
    int CustomerCount,
    int OutOfStockCount,
    int TeamMemberCount,
    int AdminCount,
    IReadOnlyList<AdminRecentSale> RecentSales,
    IReadOnlyList<AdminLowStockItem> LowStockItems,
    IReadOnlyList<AdminStockEvent> RecentStockEvents);

public sealed record ReportRow(
    DateOnly Date,
    int Orders,
    decimal Revenue,
    decimal Cost,
    decimal Profit);

public sealed record ReportSnapshot(
    IReadOnlyList<ReportRow> DailyRows,
    decimal Revenue,
    decimal Cost,
    decimal Profit,
    int UnitsSold,
    int Orders);
