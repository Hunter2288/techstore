using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Dal.Interfaces
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IReadOnlyList<Payment>> GetByOrderAsync(long orderId, CancellationToken ct = default);
    }
}
