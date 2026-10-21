USE TechStore_Catalog;
GO

SELECT fk.name, OBJECT_NAME(fk.parent_object_id) AS Child, OBJECT_NAME(fk.referenced_object_id) AS Parent
FROM sys.foreign_keys fk ORDER BY Child;

SELECT ProductId, Name, Price, Brand, PrimaryCategory FROM dbo.vw_ProductCard WHERE Name LIKE N'%Sony%';
SELECT ProductId, Name, Price FROM dbo.vw_ProductCard
ORDER BY Price, ProductId OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;

SELECT p.Name, pc.IsPrimary, pc.AddedAt
FROM dbo.ProductCategories pc
JOIN dbo.Products p ON p.ProductId = pc.ProductId
JOIN dbo.Categories c ON c.CategoryId = pc.CategoryId
WHERE c.Name = N'Електроніка';

BEGIN TRY  
    INSERT dbo.Products (Sku, Name, Price, BrandId) VALUES ('BAD-1', N'x', -1, (SELECT MIN(BrandId) FROM dbo.Brands));
END TRY BEGIN CATCH SELECT 'negative price' AS Test, ERROR_NUMBER() AS ErrNo, ERROR_MESSAGE() AS Msg; END CATCH;

BEGIN TRY  
    INSERT dbo.Products (Sku, Name, Price, BrandId) VALUES ('SM-S24', N'x', 1, (SELECT MIN(BrandId) FROM dbo.Brands));
END TRY BEGIN CATCH SELECT 'duplicate sku', ERROR_NUMBER(), ERROR_MESSAGE(); END CATCH;

BEGIN TRY  
    INSERT dbo.ProductImages (ProductId, Url, IsMain) VALUES (1, '/img/x.jpg', 1);
END TRY BEGIN CATCH SELECT 'second main image', ERROR_NUMBER(), ERROR_MESSAGE(); END CATCH;

BEGIN TRY  
    INSERT dbo.ProductCategories (ProductId, CategoryId, IsPrimary)
    VALUES (1, (SELECT CategoryId FROM dbo.Categories WHERE Name = N'Ноутбуки'), 1);
END TRY BEGIN CATCH SELECT 'second primary category', ERROR_NUMBER(), ERROR_MESSAGE(); END CATCH;

BEGIN TRY  
    UPDATE dbo.ProductDetails SET Specs = N'не json' WHERE ProductId = 1;
END TRY BEGIN CATCH SELECT 'invalid json', ERROR_NUMBER(), ERROR_MESSAGE(); END CATCH;