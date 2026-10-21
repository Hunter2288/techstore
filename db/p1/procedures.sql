USE TechStore_Orders;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateOrder
    @CustomerId BIGINT, @City NVARCHAR(100), @Address NVARCHAR(200), @DeliveryMethod NVARCHAR(20),
    @ItemsJson NVARCHAR(MAX), @OrderId BIGINT OUTPUT, @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRAN;
        IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE CustomerId = @CustomerId AND IsDeleted = 0)
            THROW 50001, 'Customer not found', 1;

        DECLARE @Items TABLE (ProductId BIGINT PRIMARY KEY, Quantity INT NOT NULL);
        INSERT @Items
        SELECT ProductId, SUM(Quantity)
        FROM OPENJSON(@ItemsJson) WITH (ProductId BIGINT '$.productId', Quantity INT '$.quantity')
        GROUP BY ProductId;

        IF NOT EXISTS (SELECT 1 FROM @Items) THROW 50002, 'Order has no items', 1;
        IF EXISTS (SELECT 1 FROM @Items i
                   LEFT JOIN dbo.ProductRefs p ON p.ProductId = i.ProductId
                   WHERE p.ProductId IS NULL OR p.IsActive = 0)
            THROW 50003, 'Unknown or inactive product', 1;

        INSERT dbo.Orders (CustomerId, CreatedBy) VALUES (@CustomerId, @CreatedBy);
        SET @OrderId = SCOPE_IDENTITY();

        INSERT dbo.OrderItems (OrderId, ProductId, ProductName, UnitPrice, Quantity)
        SELECT @OrderId, p.ProductId, p.Name, p.Price, i.Quantity      -- знімок
        FROM @Items i JOIN dbo.ProductRefs p ON p.ProductId = i.ProductId;

        UPDATE dbo.Orders
        SET TotalAmount = (SELECT SUM(LineTotal) FROM dbo.OrderItems WHERE OrderId = @OrderId)
        WHERE OrderId = @OrderId;

        INSERT dbo.DeliveryInfos (OrderId, City, Address, Method)
        VALUES (@OrderId, @City, @Address, @DeliveryMethod);

        INSERT dbo.OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedBy, Comment)
        VALUES (@OrderId, NULL, 'Submitted', ISNULL(@CreatedBy, 'system'), N'Замовлення створено');
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ChangeOrderStatus
    @OrderId BIGINT, @NewStatus NVARCHAR(20), @ChangedBy NVARCHAR(100), @Comment NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRAN;
        DECLARE @Old NVARCHAR(20);
        SELECT @Old = Status FROM dbo.Orders WITH (UPDLOCK, HOLDLOCK)
        WHERE OrderId = @OrderId AND IsDeleted = 0;

        IF @Old IS NULL THROW 50010, 'Order not found', 1;
        IF NOT (   (@Old = 'Submitted'          AND @NewStatus IN ('AwaitingValidation','Cancelled'))
                OR (@Old = 'AwaitingValidation' AND @NewStatus IN ('StockConfirmed','Cancelled'))
                OR (@Old = 'StockConfirmed'     AND @NewStatus IN ('Paid','Cancelled'))
                OR (@Old = 'Paid'               AND @NewStatus = 'Shipped')
                OR (@Old = 'Shipped'            AND @NewStatus = 'Delivered'))
            THROW 50011, 'Invalid status transition', 1;

        IF @NewStatus = 'Paid' AND NOT EXISTS
           (SELECT 1 FROM dbo.PaymentRecords WHERE OrderId = @OrderId AND Status = 'Succeeded')
            THROW 50012, 'No successful payment for this order', 1;

        UPDATE dbo.Orders
        SET Status = @NewStatus, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @ChangedBy
        WHERE OrderId = @OrderId;

        INSERT dbo.OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedBy, Comment)
        VALUES (@OrderId, @Old, @NewStatus, @ChangedBy, @Comment);
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ConfirmOrder @OrderId BIGINT, @ChangedBy NVARCHAR(100) = 'system'
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRAN;
        DECLARE @Status NVARCHAR(20) =
            (SELECT Status FROM dbo.Orders WITH (UPDLOCK) WHERE OrderId = @OrderId AND IsDeleted = 0);
        IF @Status IS NULL THROW 50001, 'Order not found', 1;
        IF @Status NOT IN ('Submitted','AwaitingValidation')
            THROW 50002, 'Stock can be confirmed only for a new order', 1;

        IF @Status = 'Submitted'
            EXEC dbo.usp_ChangeOrderStatus @OrderId, 'AwaitingValidation', @ChangedBy, N'Перевірка наявності';
        EXEC dbo.usp_ChangeOrderStatus @OrderId, 'StockConfirmed', @ChangedBy, N'Наявність підтверджено';
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApplyPromotion @OrderId BIGINT, @Code NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRAN;
        DECLARE @Status NVARCHAR(20), @PromoId BIGINT, @Pct DECIMAL(5,2), @Items DECIMAL(12,2), @Disc DECIMAL(10,2);

        SELECT @Status = Status FROM dbo.Orders WITH (UPDLOCK, HOLDLOCK) WHERE OrderId = @OrderId AND IsDeleted = 0;
        IF @Status IS NULL THROW 50020, 'Order not found', 1;
        IF @Status <> 'Submitted' THROW 50021, 'Promotion can be applied only to a Submitted order', 1;

        SELECT @PromoId = PromotionId, @Pct = DiscountPercent
        FROM dbo.Promotions
        WHERE Code = @Code AND IsActive = 1 AND SYSUTCDATETIME() BETWEEN ValidFrom AND ValidTo;
        IF @PromoId IS NULL THROW 50022, 'Promotion not found or expired', 1;
        IF EXISTS (SELECT 1 FROM dbo.OrderPromotions WHERE OrderId = @OrderId AND PromotionId = @PromoId)
            THROW 50023, 'Promotion already applied', 1;

        SELECT @Items = SUM(LineTotal) FROM dbo.OrderItems WHERE OrderId = @OrderId;
        SET @Disc = ROUND(@Items * @Pct / 100, 2);
        INSERT dbo.OrderPromotions (OrderId, PromotionId, DiscountAmount) VALUES (@OrderId, @PromoId, @Disc);

        DECLARE @Total DECIMAL(12,2) =
            @Items - (SELECT SUM(DiscountAmount) FROM dbo.OrderPromotions WHERE OrderId = @OrderId);
        IF @Total < 0 THROW 50024, 'Total discount exceeds order amount', 1;

        UPDATE dbo.Orders SET TotalAmount = @Total, UpdatedAt = SYSUTCDATETIME() WHERE OrderId = @OrderId;
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RegisterPayment
    @OrderId BIGINT, @Amount DECIMAL(12,2), @Method NVARCHAR(20), @ExternalTxnId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRAN;
        DECLARE @Status NVARCHAR(20), @Total DECIMAL(12,2);
        SELECT @Status = Status, @Total = TotalAmount
        FROM dbo.Orders WITH (UPDLOCK, HOLDLOCK) WHERE OrderId = @OrderId AND IsDeleted = 0;

        IF @Status IS NULL THROW 50030, 'Order not found', 1;
        IF @Status <> 'StockConfirmed' THROW 50031, 'Order must be StockConfirmed before payment', 1;
        IF @Amount <> @Total THROW 50032, 'Payment amount does not match order total', 1;

        INSERT dbo.PaymentRecords (OrderId, Amount, Method, Status, ExternalTxnId, PaidAt)
        VALUES (@OrderId, @Amount, @Method, 'Succeeded', @ExternalTxnId, SYSUTCDATETIME());

        EXEC dbo.usp_ChangeOrderStatus @OrderId, 'Paid', 'payment-gateway', N'Оплата отримана';
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetOrderDetails @OrderId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.OrderId, o.Status, o.TotalAmount, o.CreatedAt, c.FullName, c.Email,
           d.City, d.Address, d.Method, d.TrackingNumber
    FROM dbo.Orders o
    JOIN dbo.Customers c ON c.CustomerId = o.CustomerId
    LEFT JOIN dbo.DeliveryInfos d ON d.OrderId = o.OrderId
    WHERE o.OrderId = @OrderId AND o.IsDeleted = 0;

    SELECT ProductId, ProductName, UnitPrice, Quantity, LineTotal FROM dbo.OrderItems WHERE OrderId = @OrderId;
    SELECT OldStatus, NewStatus, ChangedAt, ChangedBy, Comment FROM dbo.OrderStatusHistory WHERE OrderId = @OrderId ORDER BY ChangedAt;
    SELECT p.Code, op.DiscountAmount FROM dbo.OrderPromotions op JOIN dbo.Promotions p ON p.PromotionId = op.PromotionId WHERE op.OrderId = @OrderId;
END
GO

CREATE OR ALTER VIEW dbo.vw_OrderSummary AS
SELECT o.OrderId, o.Status, o.TotalAmount, o.CreatedAt,
       c.CustomerId, c.FullName, c.Email,
       d.City, d.Method AS DeliveryMethod,
       COUNT(i.OrderItemId) AS ItemsCount
FROM dbo.Orders o
JOIN dbo.Customers c ON c.CustomerId = o.CustomerId
LEFT JOIN dbo.DeliveryInfos d ON d.OrderId = o.OrderId
LEFT JOIN dbo.OrderItems i ON i.OrderId = o.OrderId
WHERE o.IsDeleted = 0
GROUP BY o.OrderId, o.Status, o.TotalAmount, o.CreatedAt, c.CustomerId, c.FullName, c.Email, d.City, d.Method;
GO