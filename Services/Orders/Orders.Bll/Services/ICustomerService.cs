using Orders.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Bll.Services
{
    public interface ICustomerService
    {
        Task<CustomerDto> GetByIdAsync(long id, CancellationToken ct);
        Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct);
        Task<CustomerDto> CreateAsync(CustomerInputDto input, CancellationToken ct);
        Task UpdateAsync(long id, CustomerInputDto input, CancellationToken ct);
        Task DeleteAsync(long id, CancellationToken ct);
    }
}
