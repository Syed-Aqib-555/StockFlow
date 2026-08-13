# Money handling

All costs, prices, discounts, totals, revenue, and profit use `decimal` with database precision `decimal(18,2)`. Floating-point types are intentionally avoided because binary rounding errors are unacceptable for invoices.
