using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Dal.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default);
        Task<long> AddAsync(Customer c, CancellationToken ct = default);
        Task<bool> UpdateAsync(Customer c, CancellationToken ct = default);
        Task<bool> DeleteAsync(long id, string? deletedBy, CancellationToken ct = default); // soft delete
    }
}
