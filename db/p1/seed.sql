USE TechStore_Orders;
GO
INSERT dbo.Customers (FullName, Email, Phone)
SELECT v.FullName, v.Email, v.Phone
FROM (VALUES (N'Іван Петренко','ivan@example.com','+380501112233'),
             (N'Олена Коваль','olena@example.com','+380671112244')) v(FullName, Email, Phone)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Customers c WHERE c.Email = v.Email);

MERGE dbo.ProductRefs AS t
USING (VALUES (1, N'Смартфон Samsung Galaxy S24', 31999.00),
              (2, N'Ноутбук Lenovo IdeaPad 5',    27999.00),
              (3, N'Навушники Sony WH-1000XM5',   12999.00)) AS s(ProductId, Name, Price)
ON t.ProductId = s.ProductId
WHEN NOT MATCHED THEN INSERT (ProductId, Name, Price) VALUES (s.ProductId, s.Name, s.Price);

IF NOT EXISTS (SELECT 1 FROM dbo.Promotions WHERE Code = 'WELCOME5')
    INSERT dbo.Promotions (Code, DiscountPercent, ValidFrom, ValidTo)
    VALUES ('WELCOME5', 5.00, '2026-01-01', '2027-12-31');

IF NOT EXISTS (SELECT 1 FROM dbo.Orders)
BEGIN
    DECLARE @cid BIGINT = (SELECT CustomerId FROM dbo.Customers WHERE Email = 'ivan@example.com');
    DECLARE @a BIGINT, @b BIGINT, @total DECIMAL(12,2);

    EXEC dbo.usp_CreateOrder @CustomerId=@cid, @City=N'Чернівці', @Address=N'вул. Головна, 1',
         @DeliveryMethod='Courier', @ItemsJson=N'[{"productId":1,"quantity":1},{"productId":3,"quantity":2}]',
         @OrderId=@a OUTPUT, @CreatedBy='seed';
    EXEC dbo.usp_ApplyPromotion @OrderId=@a, @Code='WELCOME5';
    EXEC dbo.usp_ConfirmOrder   @OrderId=@a, @ChangedBy='manager1';
    SELECT @total = TotalAmount FROM dbo.Orders WHERE OrderId = @a;
    EXEC dbo.usp_RegisterPayment @OrderId=@a, @Amount=@total, @Method='Card', @ExternalTxnId='TXN-0001';

    EXEC dbo.usp_CreateOrder @CustomerId=@cid, @City=N'Чернівці', @Address=N'вул. Головна, 1',
         @DeliveryMethod='Pickup', @ItemsJson=N'[{"productId":2,"quantity":1}]',
         @OrderId=@b OUTPUT, @CreatedBy='seed';
END
GO