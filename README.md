# StockFlow

StockFlow is a complete single-store inventory, POS, order, customer, invoice, and reporting system built for SAH Store and other small fashion retailers.

## Highlights

- .NET 10 Blazor Web App with Interactive Server rendering
- ASP.NET Core Identity with Admin and Cashier roles
- SQL Server LocalDB and Entity Framework Core migrations
- Product variants for size, color, SKU, cost, price, and quantity
- Audited stock receipts, adjustments, damages, returns, and sales
- Transaction-safe POS checkout with overselling protection
- Printable invoices, customer history, dashboards, and profit reports
- Responsive custom interface using Bootstrap 5 and Chart.js

## Start locally

1. Open `StockFlow.slnx` in Visual Studio Community 2026.
2. Confirm the `https` launch profile is selected.
3. Press `F5`. The database migrates and demo data is created automatically.

Demo accounts:

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@stockflow.local` | `StockFlow123!` |
| Cashier | `cashier@stockflow.local` | `StockFlow123!` |

These credentials are for local demonstration only. Change them before any real deployment.

## Quality checks

```powershell
dotnet restore --configfile NuGet.Config
dotnet build StockFlow.slnx --no-restore
dotnet test StockFlow.slnx --no-restore
```

See [docs/README.md](docs/README.md) for the complete product and developer guide.
