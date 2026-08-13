# Troubleshooting

If startup fails, confirm LocalDB is installed and the connection string is valid. If restore fails, use the repository `NuGet.Config`. If migrations changed, rebuild before running. A locked output file usually means a prior StockFlow process is still running.
