using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Orders.Bll.Dto
{
    public class CreateOrderItemDto
    {
        [Range(1, long.MaxValue)] public long ProductId { get; set; }
        [Range(1, 1000)] public int Quantity { get; set; }
    }

    public class CreateOrderDto
    {
        [Range(1, long.MaxValue)] public long CustomerId { get; set; }
        [Required, StringLength(100)] public string City { get; set; } = "";
        [Required, StringLength(200)] public string Address { get; set; } = "";
        [Required, RegularExpression("^(Courier|Pickup|Post)$")] public string DeliveryMethod { get; set; } = "";
        [Required, MinLength(1)] public List<CreateOrderItemDto> Items { get; set; } = new();
    }

}
