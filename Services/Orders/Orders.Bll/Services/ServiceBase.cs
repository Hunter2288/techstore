using Orders.Dal.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Bll.Services
{
    public abstract class ServiceBase(IUnitOfWork uow)
    {
        protected readonly IUnitOfWork Uow = uow;

        protected async Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct,
            IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            await Uow.BeginTransactionAsync(level, ct);
            try
            {
                var result = await work();
                await Uow.CommitAsync(ct);
                return result;
            }
            catch
            {
                await Uow.RollbackAsync(ct);
                throw;
            }
        }
    }
}
