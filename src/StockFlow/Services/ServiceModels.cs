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
