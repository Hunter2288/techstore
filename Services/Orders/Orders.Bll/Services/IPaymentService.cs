using Orders.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Bll.Services
{
    public interface IPaymentService
    {
        Task<IReadOnlyList<PaymentDto>> GetByOrderAsync(long orderId, CancellationToken ct);
        Task<PaymentDto> RegisterAsync(long orderId, RegisterPaymentDto input, CancellationToken ct);
    }
}
