using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Bll.Dto
{
    
    public record CustomerDto(long CustomerId, string FullName, string Email, string? Phone, DateTime CreatedAt);
    public record OrderItemDto(long ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
}
