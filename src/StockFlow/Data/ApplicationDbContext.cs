using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockFlow.Models;

namespace StockFlow.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<ProductVariant>().HasIndex(x => x.Sku).IsUnique();
        builder.Entity<ProductVariant>().HasIndex(x => x.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");
        builder.Entity<Sale>().HasIndex(x => x.SaleNumber).IsUnique();
        builder.Entity<Customer>().HasIndex(x => x.Phone);

        builder.Entity<ProductVariant>().Property(x => x.CostPrice).HasPrecision(18, 2);
        builder.Entity<ProductVariant>().Property(x => x.SellingPrice).HasPrecision(18, 2);
        builder.Entity<Sale>().Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Entity<Sale>().Property(x => x.Discount).HasPrecision(18, 2);
        builder.Entity<Sale>().Property(x => x.Total).HasPrecision(18, 2);
        builder.Entity<SaleItem>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Entity<SaleItem>().Property(x => x.UnitCost).HasPrecision(18, 2);

        builder.Entity<Product>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Product>()
            .HasOne(x => x.Supplier)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ProductVariant>()
            .HasOne(x => x.Product)
            .WithMany(x => x.Variants)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<StockTransaction>()
            .HasOne(x => x.Variant)
            .WithMany(x => x.StockTransactions)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SaleItem>()
            .HasOne(x => x.Variant)
            .WithMany(x => x.SaleItems)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SaleItem>()
            .HasOne(x => x.Sale)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
