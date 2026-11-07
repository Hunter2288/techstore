using AutoMapper;
using Orders.Bll.Dto;
using Orders.Dal.Interfaces;
using Orders.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Orders.Bll.Services
{
    public class OrderService(IUnitOfWork uow, IMapper mapper, ILogger<OrderService>? logger = null) : ServiceBase(uow), IOrderService
    {
        private static readonly ActivitySource ActivitySource = new("Orders.Api");
        public async Task<OrderDto> GetByIdAsync(long id, CancellationToken ct)
            => mapper.Map<OrderDto>(await Uow.Orders.GetWithItemsAsync(id, ct)
                ?? throw new NotFoundException($"Order {id} not found"));

        public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken ct)
            => mapper.Map<List<OrderDto>>(await Uow.Orders.GetAllAsync(ct));

        
        public async Task<OrderDto> CreateAsync(CreateOrderDto input, CancellationToken ct)
        {
            using var activity = ActivitySource.StartActivity("orders.create");
            activity?.SetTag("order.customer_id", input.CustomerId);
            activity?.SetTag("order.item_count", input.Items.Count);
            var itemsJson = JsonSerializer.Serialize(
                input.Items.Select(i => new { productId = i.ProductId, quantity = i.Quantity }));

            var orderId = await InTransactionAsync(() => Uow.Orders.CreateWithItemsAsync(
                input.CustomerId, input.City, input.Address, input.DeliveryMethod,
                itemsJson, "api", ct), ct);

            var created = await GetByIdAsync(orderId, ct);
            activity?.SetTag("order.id", orderId);
            activity?.SetTag("order.total_amount", created.TotalAmount);
            logger?.LogInformation("Order {OrderId} created for customer {CustomerId} with {ItemsCount} items and amount {Amount}",
                orderId, created.CustomerId, created.Items.Count, created.TotalAmount);
            return created;
        }

        public async Task ConfirmOrderAsync(long orderId, CancellationToken ct)
        {
            using var activity = ActivitySource.StartActivity("orders.confirm");
            activity?.SetTag("order.id", orderId);
            await InTransactionAsync(async () =>
            {
                var order = await Uow.Orders.GetWithItemsAsync(orderId, ct)
                            ?? throw new NotFoundException($"Order {orderId} not found");
                if (order.Items.Count == 0)
                    throw new BusinessConflictException($"Order {orderId} has no items");

                await Uow.Orders.ConfirmAsync(orderId, "api", ct);
                return true;
            }, ct);
            logger?.LogInformation("Order {OrderId} confirmed with status {Status}", orderId, "StockConfirmed");
        }

        public async Task DeleteAsync(long id, CancellationToken ct)
        {
            if (!await Uow.Orders.DeleteAsync(id, ct))
                throw new NotFoundException($"Order {id} not found");
        }
    }
}
