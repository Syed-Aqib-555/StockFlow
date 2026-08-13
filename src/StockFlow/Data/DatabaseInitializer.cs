using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockFlow.Models;

namespace StockFlow.Data;

public static class DatabaseInitializer
{
    public const string AdminEmail = "admin@stockflow.local";
    public const string CashierEmail = "cashier@stockflow.local";
    public const string DemoPassword = "StockFlow123!";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var environment = services.GetRequiredService<IHostEnvironment>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var role in new[] { "Admin", "Cashier" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var admin = await EnsureUserAsync(userManager, AdminEmail, "Admin", environment.IsDevelopment());
        await EnsureUserAsync(userManager, CashierEmail, "Cashier", environment.IsDevelopment());

        if (await db.Products.AnyAsync())
        {
            return;
        }

        var streetwear = new Category { Name = "Men's Streetwear" };
        var essentials = new Category { Name = "Everyday Essentials" };
        var accessories = new Category { Name = "Accessories" };
        var supplier = new Supplier
        {
            Name = "SAH Apparel Works",
            Phone = "+92 300 555 0147",
            Email = "orders@sahapparel.local"
        };

        var products = new[]
        {
            NewProduct("No Limits No Fear T-Shirt", streetwear, supplier, "Heavy cotton statement tee with an oversized streetwear fit.",
                "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=640&q=80",
                ("M", "Black", "NLF-BLK-M", 8m, 22m, 12),
                ("L", "Black", "NLF-BLK-L", 8m, 22m, 10),
                ("XL", "Navy", "NLF-NVY-XL", 9m, 24m, 8)),
            NewProduct("SAH Core Logo Tee", essentials, supplier, "Soft combed-cotton essential with the signature SAH mark.",
                "https://images.unsplash.com/photo-1583743814966-8936f5b7be1a?auto=format&fit=crop&w=640&q=80",
                ("S", "White", "CORE-WHT-S", 7m, 19m, 4),
                ("M", "White", "CORE-WHT-M", 7m, 19m, 15),
                ("L", "Olive", "CORE-OLV-L", 7.5m, 20m, 7)),
            NewProduct("Midnight Oversized Hoodie", streetwear, supplier, "Brushed fleece hoodie with dropped shoulders and tonal embroidery.",
                "https://images.unsplash.com/photo-1556821840-3a63f95609a7?auto=format&fit=crop&w=640&q=80",
                ("M", "Charcoal", "MID-CHR-M", 19m, 46m, 6),
                ("L", "Charcoal", "MID-CHR-L", 19m, 46m, 3),
                ("XL", "Black", "MID-BLK-XL", 20m, 49m, 2)),
            NewProduct("Essential Ribbed Tank", essentials, supplier, "Clean ribbed layer built for warm days and easy styling.",
                "https://images.unsplash.com/photo-1503341504253-dff4815485f1?auto=format&fit=crop&w=640&q=80",
                ("S", "Stone", "RIB-STN-S", 5m, 15m, 14),
                ("M", "Black", "RIB-BLK-M", 5m, 15m, 11)),
            NewProduct("Canvas Street Cap", accessories, supplier, "Six-panel canvas cap with adjustable metal clasp.",
                "https://images.unsplash.com/photo-1521369909029-2afed882baee?auto=format&fit=crop&w=640&q=80",
                ("One Size", "Black", "CAP-BLK-OS", 6m, 18m, 9))
        };

        db.Products.AddRange(products);
        db.Customers.AddRange(
            new Customer { Name = "Ayesha Khan", Phone = "+92 301 442 9081", Email = "ayesha@example.com" },
            new Customer { Name = "Hamza Ali", Phone = "+92 333 671 2204", Email = "hamza@example.com" },
            new Customer { Name = "Sara Ahmed", Phone = "+92 321 190 7412", Email = "sara@example.com" });
        await db.SaveChangesAsync();

        foreach (var variant in products.SelectMany(x => x.Variants))
        {
            db.StockTransactions.Add(new StockTransaction
            {
                VariantId = variant.Id,
                Type = StockTransactionType.Received,
                QuantityChange = variant.QuantityOnHand,
                QuantityAfter = variant.QuantityOnHand,
                Date = DateTime.UtcNow.AddDays(-20),
                Notes = "Opening stock from SAH Apparel Works",
                UserId = admin.Id
            });
        }

        var customers = await db.Customers.ToListAsync();
        var variants = products.SelectMany(x => x.Variants).ToList();
        for (var dayOffset = 6; dayOffset >= 0; dayOffset--)
        {
            var lines = new[]
            {
                (Variant: variants[(6 - dayOffset) % variants.Count], Quantity: dayOffset % 2 + 1),
                (Variant: variants[(8 - dayOffset + variants.Count) % variants.Count], Quantity: 1)
            };
            var subtotal = lines.Sum(x => x.Variant.SellingPrice * x.Quantity);
            var sale = new Sale
            {
                SaleNumber = $"SF-DEMO-{DateTime.UtcNow.AddDays(-dayOffset):yyyyMMdd}",
                CustomerId = dayOffset % 3 == 0 ? null : customers[dayOffset % customers.Count].Id,
                Date = DateTime.UtcNow.Date.AddDays(-dayOffset).AddHours(11 + dayOffset),
                PaymentMethod = dayOffset % 2 == 0 ? PaymentMethod.Cash : PaymentMethod.Card,
                Subtotal = subtotal,
                Discount = dayOffset % 3 == 0 ? 2m : 0m,
                Total = subtotal - (dayOffset % 3 == 0 ? 2m : 0m),
                CreatedByUserId = admin.Id
            };
            foreach (var line in lines)
            {
                sale.Items.Add(new SaleItem
                {
                    VariantId = line.Variant.Id,
                    Quantity = line.Quantity,
                    UnitPrice = line.Variant.SellingPrice,
                    UnitCost = line.Variant.CostPrice
                });
                line.Variant.QuantityOnHand -= line.Quantity;
                db.StockTransactions.Add(new StockTransaction
                {
                    VariantId = line.Variant.Id,
                    Type = StockTransactionType.Sale,
                    QuantityChange = -line.Quantity,
                    QuantityAfter = line.Variant.QuantityOnHand,
                    Date = sale.Date,
                    Notes = $"Demo sale {sale.SaleNumber}",
                    UserId = admin.Id
                });
            }
            db.Sales.Add(sale);
        }

        await db.SaveChangesAsync();
    }

    private static Product NewProduct(
        string name,
        Category category,
        Supplier supplier,
        string description,
        string imageUrl,
        params (string Size, string Color, string Sku, decimal Cost, decimal Price, int Stock)[] variants)
    {
        var product = new Product
        {
            Name = name,
            Category = category,
            Supplier = supplier,
            Description = description,
            ImageUrl = imageUrl
        };
        foreach (var variant in variants)
        {
            product.Variants.Add(new ProductVariant
            {
                Size = variant.Size,
                Color = variant.Color,
                Sku = variant.Sku,
                CostPrice = variant.Cost,
                SellingPrice = variant.Price,
                QuantityOnHand = variant.Stock,
                LowStockThreshold = 5
            });
        }
        return product;
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string role,
        bool restoreDemoAccess)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        if (restoreDemoAccess)
        {
            if (!await userManager.CheckPasswordAsync(user, DemoPassword))
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await userManager.ResetPasswordAsync(user, resetToken, DemoPassword);
                if (!resetResult.Succeeded)
                {
                    throw new InvalidOperationException(string.Join("; ", resetResult.Errors.Select(x => x.Description)));
                }
            }

            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);
        }

        return user;
    }
}
