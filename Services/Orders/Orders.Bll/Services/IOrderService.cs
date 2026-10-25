using Orders.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Bll.Services
{
    public interface IOrderService
    {
        Task<OrderDto> GetByIdAsync(long id, CancellationToken ct);
        Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken ct);
        Task<OrderDto> CreateAsync(CreateOrderDto input, CancellationToken ct);
        Task ConfirmOrderAsync(long orderId, CancellationToken ct);
        Task DeleteAsync(long id, CancellationToken ct);
    }
}
