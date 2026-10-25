using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(CatalogDbContext db, CancellationToken ct = default)
        {
            if (await db.Products.AnyAsync(ct)) return;            // уже наповнено

            // Sku, Name, Price, Stock, BrandId, CategoryId (довідники з HasData)
            var items = new (string Sku, string Name, decimal Price, int Stock, long BrandId, long CategoryId)[]
            {
            ("SM-S24",  "Смартфон Samsung Galaxy S24",       31999m, 15, 1, 2),
            ("LN-IP5",  "Ноутбук Lenovo IdeaPad 5",          27999m,  8, 2, 3),
            ("SN-XM5",  "Навушники Sony WH-1000XM5",         12999m, 25, 3, 4),
            ("SM-A55",  "Смартфон Samsung Galaxy A55",       18999m, 30, 1, 2),
            ("SM-S24U", "Смартфон Samsung Galaxy S24 Ultra", 52999m,  6, 1, 2),
            ("SM-A15",  "Смартфон Samsung Galaxy A15",        7999m, 40, 1, 2),
            ("LN-LG5",  "Ноутбук Lenovo Legion 5",           54999m,  5, 2, 3),
            ("LN-TP14", "Ноутбук Lenovo ThinkPad E14",       36999m,  9, 2, 3),
            ("LN-YG7",  "Ноутбук Lenovo Yoga 7",             44999m,  4, 2, 3),
            ("SN-XM4",  "Навушники Sony WH-1000XM4",          9999m, 12, 3, 4),
            ("SN-WF5",  "Навушники Sony WF-1000XM5",         10999m, 18, 3, 4),
            ("SN-SRS",  "Колонка Sony SRS-XB13",              2499m, 50, 3, 4),
            ("SN-LBT",  "Навушники Sony LinkBuds S",          7499m, 14, 3, 4),
            };

            for (var i = 0; i < items.Length; i++)
            {
                var (sku, name, price, stock, brandId, categoryId) = items[i];
                var product = new Product(name, sku, price, brandId, "seed");
                product.AddStock(stock, "seed");

                product.Images.Add(new ProductImage { Url = $"/img/{sku.ToLowerInvariant()}-1.jpg", IsMain = true, SortOrder = 0 });
                product.ProductCategories.Add(new ProductCategory { CategoryId = categoryId, IsPrimary = true, SortOrder = 0 });
                product.ProductCategories.Add(new ProductCategory { CategoryId = 1, IsPrimary = false, SortOrder = 1 });

                if (i < 3)                                            // деталі лише для перших трьох
                    product.SetDetail(new ProductDetail
                    {
                        Description = name,
                        WarrantyMonths = i == 0 ? 24 : 12,
                        WeightKg = i switch { 0 => 0.17m, 1 => 1.65m, _ => 0.25m },
                        Specs = i switch
                        {
                            0 => """{"ram":"8GB","storage":"256GB","screen":"6.2"}""",
                            1 => """{"cpu":"Ryzen 5","ram":"16GB","ssd":"512GB"}""",
                            _ => """{"battery":"30h","anc":true}"""
                        }
                    });

                db.Products.Add(product);
            }

            await db.SaveChangesAsync(ct);                            // один виклик, одна транзакція
        }
    }
}
