using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Dal.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetWithItemsAsync(long orderId, CancellationToken ct = default);
        Task<long> CreateWithItemsAsync(long customerId, string city, string address, string method,
                                        string itemsJson, string? createdBy, CancellationToken ct = default); // usp_CreateOrder
        Task ConfirmAsync(long orderId, string changedBy, CancellationToken ct = default);                    // usp_ConfirmOrder
        Task ChangeStatusAsync(long orderId, string newStatus, string changedBy, string? comment,
                               CancellationToken ct = default);                                                // usp_ChangeOrderStatus
    }
}
