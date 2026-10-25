using AutoMapper;
using Orders.Bll.Dto;
using Orders.Dal.Interfaces;
using Orders.Domain.Exceptions;
using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Bll.Services
{
    public class PaymentService(IUnitOfWork uow, IMapper mapper) : ServiceBase(uow), IPaymentService
    {
        public async Task<IReadOnlyList<PaymentDto>> GetByOrderAsync(long orderId, CancellationToken ct)
        {
            _ = await Uow.Orders.GetByIdAsync(orderId, ct)
                ?? throw new NotFoundException($"Order {orderId} not found");
            return mapper.Map<List<PaymentDto>>(await Uow.Payments.GetByOrderAsync(orderId, ct));
        }

        public async Task<PaymentDto> RegisterAsync(long orderId, RegisterPaymentDto input, CancellationToken ct)
        {
            var paymentId = await InTransactionAsync(async () =>
            {
                var order = await Uow.Orders.GetByIdAsync(orderId, ct)
                            ?? throw new NotFoundException($"Order {orderId} not found");
                if (order.Status != "StockConfirmed")
                    throw new BusinessConflictException("Order must be StockConfirmed before payment");
                if (input.Amount != order.TotalAmount)
                    throw new BusinessConflictException("Payment amount does not match order total");

                var id = await Uow.Payments.AddAsync(new Payment
                {
                    OrderId = orderId,
                    Amount = input.Amount,
                    Method = input.Method,
                    Status = "Succeeded",
                    ExternalTxnId = input.ExternalTxnId,
                    PaidAt = DateTime.UtcNow
                }, ct);

               
                await Uow.Orders.ChangeStatusAsync(orderId, "Paid", "payment-gateway", "Оплата отримана", ct);
                return id;
            }, ct);

            return mapper.Map<PaymentDto>(await Uow.Payments.GetByIdAsync(paymentId, ct));
        }
    }
}
