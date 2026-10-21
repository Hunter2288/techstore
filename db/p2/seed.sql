USE TechStore_Catalog;
GO

INSERT dbo.Brands (Name, Country)
SELECT v.Name, v.Country
FROM (VALUES (N'Samsung',N'South Korea'),(N'Lenovo',N'China'),(N'Sony',N'Japan')) v(Name, Country)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Brands b WHERE b.Name = v.Name);

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = N'Електроніка')
    INSERT dbo.Categories (Name, ParentCategoryId) VALUES (N'Електроніка', NULL);

INSERT dbo.Categories (Name, ParentCategoryId)
SELECT v.Name, root.CategoryId
FROM (VALUES (N'Смартфони'),(N'Ноутбуки'),(N'Аудіо')) v(Name)
CROSS JOIN (SELECT CategoryId FROM dbo.Categories WHERE Name = N'Електроніка') root
WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories c WHERE c.Name = v.Name);

INSERT dbo.Products (Sku, Name, Price, StockQuantity, BrandId, CreatedBy)
SELECT v.Sku, v.Name, v.Price, v.Stock, b.BrandId, 'seed'
FROM (VALUES ('SM-S24', N'Смартфон Samsung Galaxy S24', 31999.00, 15, N'Samsung'),
             ('LN-IP5', N'Ноутбук Lenovo IdeaPad 5',    27999.00, 8,  N'Lenovo'),
             ('SN-XM5', N'Навушники Sony WH-1000XM5',   12999.00, 25, N'Sony')) v(Sku, Name, Price, Stock, Brand)
JOIN dbo.Brands b ON b.Name = v.Brand
WHERE NOT EXISTS (SELECT 1 FROM dbo.Products p WHERE p.Sku = v.Sku);

INSERT dbo.ProductDetails (ProductId, Description, WarrantyMonths, WeightKg, Specs)
SELECT p.ProductId, v.Descr, v.Warranty, v.Weight, v.Specs
FROM (VALUES ('SM-S24', N'Флагманський смартфон', 24, 0.17, N'{"ram":"8GB","storage":"256GB","screen":"6.2"}'),
             ('LN-IP5', N'Ноутбук для роботи й навчання', 12, 1.65, N'{"cpu":"Ryzen 5","ram":"16GB","ssd":"512GB"}'),
             ('SN-XM5', N'Бездротові навушники з ANC', 12, 0.25, N'{"battery":"30h","anc":true}')) v(Sku, Descr, Warranty, Weight, Specs)
JOIN dbo.Products p ON p.Sku = v.Sku
WHERE NOT EXISTS (SELECT 1 FROM dbo.ProductDetails d WHERE d.ProductId = p.ProductId);

INSERT dbo.ProductImages (ProductId, Url, IsMain, SortOrder)
SELECT p.ProductId, v.Url, v.IsMain, v.Sort
FROM (VALUES ('SM-S24','/img/s24-1.jpg',1,0), ('SM-S24','/img/s24-2.jpg',0,1),
             ('LN-IP5','/img/ip5-1.jpg',1,0), ('SN-XM5','/img/xm5-1.jpg',1,0)) v(Sku, Url, IsMain, Sort)
JOIN dbo.Products p ON p.Sku = v.Sku
WHERE NOT EXISTS (SELECT 1 FROM dbo.ProductImages i WHERE i.ProductId = p.ProductId AND i.Url = v.Url);

INSERT dbo.ProductCategories (ProductId, CategoryId, IsPrimary, SortOrder)
SELECT p.ProductId, c.CategoryId, v.IsPrimary, v.Sort
FROM (VALUES ('SM-S24',N'Смартфони',1,0), ('SM-S24',N'Електроніка',0,1),
             ('LN-IP5',N'Ноутбуки',1,0),  ('LN-IP5',N'Електроніка',0,1),
             ('SN-XM5',N'Аудіо',1,0),     ('SN-XM5',N'Електроніка',0,1)) v(Sku, Cat, IsPrimary, Sort)
JOIN dbo.Products p ON p.Sku = v.Sku
JOIN dbo.Categories c ON c.Name = v.Cat
WHERE NOT EXISTS (SELECT 1 FROM dbo.ProductCategories x WHERE x.ProductId = p.ProductId AND x.CategoryId = c.CategoryId);
GO

