using Dapper;
using Microsoft.Data.SqlClient;
using Orders.Dal.Interfaces;
using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Dal.Repositories
{
    public sealed class PaymentRepository : BaseDapperRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(SqlConnection c, Func<SqlTransaction?> tx)
            : base(c, tx, "dbo.PaymentRecords", "PaymentId", softDelete: false) { }

        public override async Task<long> AddAsync(Payment p, CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            return await Guard(() => Connection.ExecuteScalarAsync<long>(new CommandDefinition(
                @"INSERT dbo.PaymentRecords (OrderId, Amount, Method, Status, ExternalTxnId, PaidAt)
              OUTPUT INSERTED.PaymentId
              VALUES (@OrderId, @Amount, @Method, @Status, @ExternalTxnId, @PaidAt)",
                p, Transaction, cancellationToken: ct)));
        }

        public async Task<IReadOnlyList<Payment>> GetByOrderAsync(long orderId, CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            var rows = await Guard(() => Connection.QueryAsync<Payment>(new CommandDefinition(
                "SELECT * FROM dbo.PaymentRecords WHERE OrderId = @OrderId ORDER BY PaymentId",
                new { OrderId = orderId }, Transaction, cancellationToken: ct)));
            return rows.ToList();
        }
    }
}
