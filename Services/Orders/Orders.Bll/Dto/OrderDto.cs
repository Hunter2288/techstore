using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Bll.Dto
{
    public record OrderDto(long OrderId, long CustomerId, string Status, decimal TotalAmount,
                           DateTime CreatedAt, IReadOnlyList<OrderItemDto> Items);
}
