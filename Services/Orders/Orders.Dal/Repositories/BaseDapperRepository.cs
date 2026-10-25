using Dapper;
using Microsoft.Data.SqlClient;
using Orders.Dal.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Dal.Repositories
{
    public abstract class BaseDapperRepository<T> : RepositoryBase, IGenericRepository<T> where T : class
    {
        private readonly string _table;      
        private readonly string _keyColumn;
        private readonly bool _softDelete;

        protected BaseDapperRepository(SqlConnection c, Func<SqlTransaction?> tx,
                                       string table, string keyColumn, bool softDelete)
            : base(c, tx)
        {
            _table = table; _keyColumn = keyColumn; _softDelete = softDelete;
        }

        private string AliveFilter => _softDelete ? " AND IsDeleted = 0" : "";

        public virtual async Task<T?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            var sql = $"SELECT * FROM {_table} WHERE {_keyColumn} = @Id{AliveFilter}";
            return await Guard(() => Connection.QuerySingleOrDefaultAsync<T>(
                new CommandDefinition(sql, new { Id = id }, Transaction, cancellationToken: ct)));
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            var sql = $"SELECT * FROM {_table} WHERE 1 = 1{AliveFilter}";
            var rows = await Guard(() => Connection.QueryAsync<T>(
                new CommandDefinition(sql, transaction: Transaction, cancellationToken: ct)));
            return rows.ToList();
        }

        public abstract Task<long> AddAsync(T entity, CancellationToken ct = default);

        public virtual async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
        {
            await EnsureOpenAsync(ct);
            var sql = _softDelete
                ? $"UPDATE {_table} SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME() WHERE {_keyColumn} = @Id AND IsDeleted = 0"
                : $"DELETE FROM {_table} WHERE {_keyColumn} = @Id";
            var affected = await Guard(() => Connection.ExecuteAsync(
                new CommandDefinition(sql, new { Id = id }, Transaction, cancellationToken: ct)));
            return affected > 0;
        }
    }
}
