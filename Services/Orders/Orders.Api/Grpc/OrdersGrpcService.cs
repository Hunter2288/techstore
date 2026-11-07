using System.Globalization;
using Orders.Bll.Services;
using Orders.Domain.Exceptions;
using Orders.Grpc;
using Grpc.Core;

namespace Orders.Api.Grpc;

public sealed class OrdersGrpcService(IOrderService orders) : OrdersService.OrdersServiceBase
{
    public override async Task<OrderReply> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be positive"));
        try
        {
            var order = await orders.GetByIdAsync(request.Id, context.CancellationToken);
            var reply = new OrderReply
            {
                Id = order.OrderId,
                CustomerId = order.CustomerId,
                Status = order.Status,
                TotalAmount = order.TotalAmount.ToString(CultureInfo.InvariantCulture),
                CreatedAt = order.CreatedAt.ToString("O", CultureInfo.InvariantCulture)
            };
            reply.Items.AddRange(order.Items.Select(item => new OrderItemReply
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice.ToString(CultureInfo.InvariantCulture),
                Quantity = item.Quantity,
                LineTotal = item.LineTotal.ToString(CultureInfo.InvariantCulture)
            }));
            return reply;
        }
        catch (NotFoundException exception)
        {
            throw new RpcException(new Status(StatusCode.NotFound, exception.Message));
        }
    }
}
