using Microsoft.Data.SqlClient;
using Orders.Dal.Interfaces;
using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Orders.Dal.Repositories
{
    public sealed class CustomerRepository : RepositoryBase, ICustomerRepository
    {
        public CustomerRepository(SqlConnection c, Func<SqlTransaction?> tx) : base(c, tx) { }

        private const string Columns =
            "CustomerId, FullName, Email, Phone, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted";

        private static Customer Map(SqlDataReader r) => new()
        {
            CustomerId = r.GetInt64(0),
            FullName = r.GetString(1),
            Email = r.GetString(2),
            Phone = r.IsDBNull(3) ? null : r.GetString(3),
            CreatedAt = r.GetDateTime(4),
            CreatedBy = r.IsDBNull(5) ? null : r.GetString(5),
            UpdatedAt = r.IsDBNull(6) ? null : r.GetDateTime(6),
            UpdatedBy = r.IsDBNull(7) ? null : r.GetString(7),
            IsDeleted = r.GetBoolean(8)
        };

        public async Task<Customer?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            await using var cmd = new SqlCommand(
                $"SELECT {Columns} FROM dbo.Customers WHERE CustomerId = @Id AND IsDeleted = 0", Connection, Transaction);
            cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = id;
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? Map(reader) : null;
        }

        public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            await using var cmd = new SqlCommand(
                $"SELECT {Columns} FROM dbo.Customers WHERE IsDeleted = 0 ORDER BY CustomerId", Connection, Transaction);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var list = new List<Customer>();
            while (await reader.ReadAsync(ct)) list.Add(Map(reader));
            return list;
        }

        public Task<long> AddAsync(Customer c, CancellationToken ct = default) => Guard(async () =>
        {
            await EnsureOpenAsync(ct);
            await using var cmd = new SqlCommand(
                @"INSERT dbo.Customers (FullName, Email, Phone, CreatedBy)
              OUTPUT INSERTED.CustomerId
              VALUES (@FullName, @Email, @Phone, @CreatedBy)", Connection, Transaction);
            cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = c.FullName;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = c.Email;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = (object?)c.Phone ?? DBNull.Value;
            cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 100).Value = (object?)c.CreatedBy ?? DBNull.Value;
            return (long)(await cmd.ExecuteScalarAsync(ct))!;
        });

        public Task<bool> UpdateAsync(Customer c, CancellationToken ct = default) => Guard(async () =>
        {
            await EnsureOpenAsync(ct);
            await using var cmd = new SqlCommand(
                @"UPDATE dbo.Customers
              SET FullName = @FullName, Email = @Email, Phone = @Phone,
                  UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
              WHERE CustomerId = @Id AND IsDeleted = 0", Connection, Transaction);
            cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = c.CustomerId;
            cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = c.FullName;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = c.Email;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = (object?)c.Phone ?? DBNull.Value;
            cmd.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar, 100).Value = (object?)c.UpdatedBy ?? DBNull.Value;
            return await cmd.ExecuteNonQueryAsync(ct) > 0;
        });

        public Task<bool> DeleteAsync(long id, string? deletedBy, CancellationToken ct = default) => Guard(async () =>
        {
            await EnsureOpenAsync(ct);
            await using var cmd = new SqlCommand(
                @"UPDATE dbo.Customers
              SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @By
              WHERE CustomerId = @Id AND IsDeleted = 0", Connection, Transaction);
            cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = id;
            cmd.Parameters.Add("@By", SqlDbType.NVarChar, 100).Value = (object?)deletedBy ?? DBNull.Value;
            return await cmd.ExecuteNonQueryAsync(ct) > 0;
        });
    }
}
