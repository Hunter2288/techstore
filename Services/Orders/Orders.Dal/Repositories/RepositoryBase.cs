using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Orders.Dal.Repositories
{
    public abstract class RepositoryBase
    {
        protected readonly SqlConnection Connection;
        private readonly Func<SqlTransaction?> _currentTransaction;

        protected RepositoryBase(SqlConnection connection, Func<SqlTransaction?> currentTransaction)
        {
            Connection = connection;
            _currentTransaction = currentTransaction;
        }

        protected SqlTransaction? Transaction => _currentTransaction();

        protected async Task EnsureOpenAsync(CancellationToken ct)
        {
            if (Connection.State != ConnectionState.Open)
                await Connection.OpenAsync(ct);
        }

        protected static async Task<TResult> Guard<TResult>(Func<Task<TResult>> action)
        {
            try { return await action(); }
            catch (SqlException ex) when (SqlErrorTranslator.TryTranslate(ex, out var domain)) { throw domain!; }
        }

        protected static Task GuardVoid(Func<Task> action)
            => Guard<bool>(async () => { await action(); return true; });
    }
}
