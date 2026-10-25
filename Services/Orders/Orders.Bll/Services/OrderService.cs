using AutoMapper;
using Orders.Bll.Dto;
using Orders.Dal.Interfaces;
using Orders.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Orders.Bll.Services
{
    public class OrderService(IUnitOfWork uow, IMapper mapper) : ServiceBase(uow), IOrderService
    {
        public async Task<OrderDto> GetByIdAsync(long id, CancellationToken ct)
            => mapper.Map<OrderDto>(await Uow.Orders.GetWithItemsAsync(id, ct)
                ?? throw new NotFoundException($"Order {id} not found"));

        public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken ct)
            => mapper.Map<List<OrderDto>>(await Uow.Orders.GetAllAsync(ct));

        
        public async Task<OrderDto> CreateAsync(CreateOrderDto input, CancellationToken ct)
        {
            var itemsJson = JsonSerializer.Serialize(
                input.Items.Select(i => new { productId = i.ProductId, quantity = i.Quantity }));

            var orderId = await InTransactionAsync(() => Uow.Orders.CreateWithItemsAsync(
                input.CustomerId, input.City, input.Address, input.DeliveryMethod,
                itemsJson, "api", ct), ct);

            return await GetByIdAsync(orderId, ct);
        }

        public Task ConfirmOrderAsync(long orderId, CancellationToken ct) => InTransactionAsync(async () =>
        {
            var order = await Uow.Orders.GetWithItemsAsync(orderId, ct)
                        ?? throw new NotFoundException($"Order {orderId} not found");
            if (order.Items.Count == 0)
                throw new BusinessConflictException($"Order {orderId} has no items");

           
            await Uow.Orders.ConfirmAsync(orderId, "api", ct);
            return true;
        }, ct);

        public async Task DeleteAsync(long id, CancellationToken ct)
        {
            if (!await Uow.Orders.DeleteAsync(id, ct))
                throw new NotFoundException($"Order {id} not found");
        }
    }
}
