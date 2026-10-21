USE master;
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'orders_app')
    CREATE LOGIN orders_app WITH PASSWORD = 'ChangeMe_Str0ng!';
GO
USE TechStore_Orders;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'orders_app')
    CREATE USER orders_app FOR LOGIN orders_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO orders_app;
GRANT EXECUTE ON SCHEMA::dbo TO orders_app;
GO
