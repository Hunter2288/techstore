USE TechStore_Orders;
GO

SELECT fk.name AS FkName, OBJECT_NAME(fk.parent_object_id) AS ChildTable,
       OBJECT_NAME(fk.referenced_object_id) AS ParentTable
FROM sys.foreign_keys fk ORDER BY ChildTable;

DELETE FROM dbo.Customers WHERE Email = 'test@example.com';
INSERT dbo.Customers (FullName, Email, CreatedBy) VALUES (N'Тест Користувач', 'test@example.com', 'checks');
SELECT * FROM dbo.Customers WHERE Email = 'test@example.com';
UPDATE dbo.Customers SET Phone = '+380931234567', UpdatedAt = SYSUTCDATETIME(), UpdatedBy = 'checks' WHERE Email = 'test@example.com';
UPDATE dbo.Customers SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = 'checks' WHERE Email = 'test@example.com'; 

SELECT OrderId, Status, TotalAmount FROM dbo.vw_OrderSummary WHERE FullName LIKE N'%Петренко%' AND Status = 'Shipped';
SELECT DISTINCT OrderId FROM dbo.OrderItems WHERE ProductName LIKE N'%Sony%';

SELECT OrderId, Status, TotalAmount, CreatedAt FROM dbo.vw_OrderSummary
ORDER BY CreatedAt DESC, OrderId DESC OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;

DECLARE @b BIGINT = (SELECT MAX(OrderId) FROM dbo.Orders);
BEGIN TRY
    EXEC dbo.usp_ChangeOrderStatus @OrderId = @b, @NewStatus = 'Shipped', @ChangedBy = 'checks';
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrNo, ERROR_MESSAGE() AS Msg;
    IF XACT_STATE() <> 0 ROLLBACK;
END CATCH;
SELECT Status FROM dbo.Orders WHERE OrderId = @b;

DECLARE @a BIGINT = (SELECT MIN(OrderId) FROM dbo.Orders);
BEGIN TRY
    EXEC dbo.usp_ConfirmOrder @OrderId = @a;
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrNo, ERROR_MESSAGE() AS Msg;
    IF XACT_STATE() <> 0 ROLLBACK;
END CATCH;