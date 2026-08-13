# Concurrency

Variant rows carry a SQL Server rowversion and checkout uses serializable isolation. Together they protect against two cashiers selling the final item simultaneously. Concurrency failures should ask the cashier to reload current availability.
