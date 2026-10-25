using Dapper;
using Microsoft.Data.SqlClient;
using Orders.Dal.Interfaces;
using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Orders.Dal.Repositories
{
    public sealed class OrderRepository : BaseDapperRepository<Order>, IOrderRepository
    {
        public OrderRepository(SqlConnection c, Func<SqlTransaction?> tx)
            : base(c, tx, "dbo.Orders", "OrderId", softDelete: true) { }

        public override async Task<long> AddAsync(Order o, CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            return await Guard(() => Connection.ExecuteScalarAsync<long>(new CommandDefinition(
                @"INSERT dbo.Orders (CustomerId, CreatedBy) OUTPUT INSERTED.OrderId
              VALUES (@CustomerId, @CreatedBy)", o, Transaction, cancellationToken: ct)));
        }

        public async Task<Order?> GetWithItemsAsync(long orderId, CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            const string sql = @"
            SELECT o.OrderId, o.CustomerId, o.Status, o.TotalAmount, o.CreatedAt, o.CreatedBy,
                   o.UpdatedAt, o.UpdatedBy, o.IsDeleted,
                   i.OrderItemId, i.OrderId, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.LineTotal
            FROM dbo.Orders o LEFT JOIN dbo.OrderItems i ON i.OrderId = o.OrderId
            WHERE o.OrderId = @OrderId AND o.IsDeleted = 0";

            Order? result = null;
            await Guard(() => Connection.QueryAsync<Order, OrderItem?, Order>(
                new CommandDefinition(sql, new { OrderId = orderId }, Transaction, cancellationToken: ct),
                (order, item) =>
                {
                    result ??= order;
                    if (item is not null) result.Items.Add(item);
                    return result;
                },
                splitOn: "OrderItemId"));
            return result;
        }

        public async Task<long> CreateWithItemsAsync(long customerId, string city, string address, string method,
            string itemsJson, string? createdBy, CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            var p = new DynamicParameters();
            p.Add("@CustomerId", customerId);
            p.Add("@City", city);
            p.Add("@Address", address);
            p.Add("@DeliveryMethod", method);
            p.Add("@ItemsJson", itemsJson);
            p.Add("@CreatedBy", createdBy);
            p.Add("@OrderId", dbType: DbType.Int64, direction: ParameterDirection.Output);

            await Guard(() => Connection.ExecuteAsync(new CommandDefinition(
                "dbo.usp_CreateOrder", p, Transaction,
                commandType: CommandType.StoredProcedure, cancellationToken: ct)));
            return p.Get<long>("@OrderId");
        }

        public async Task ConfirmAsync(long orderId, string changedBy, CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            await GuardVoid(() => Connection.ExecuteAsync(new CommandDefinition(
                "dbo.usp_ConfirmOrder", new { OrderId = orderId, ChangedBy = changedBy }, Transaction,
                commandType: CommandType.StoredProcedure, cancellationToken: ct)));
        }

        public async Task ChangeStatusAsync(long orderId, string newStatus, string changedBy, string? comment,
                                            CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            await GuardVoid(() => Connection.ExecuteAsync(new CommandDefinition(
                "dbo.usp_ChangeOrderStatus",
                new { OrderId = orderId, NewStatus = newStatus, ChangedBy = changedBy, Comment = comment },
                Transaction, commandType: CommandType.StoredProcedure, cancellationToken: ct)));
        }
    }
}
