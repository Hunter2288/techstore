using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Domain.Models
{
    public class Order
    {
        public long OrderId { get; set; }
        public long CustomerId { get; set; }
        public string Status { get; set; } = "Submitted";
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public byte[]? RowVer { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }
}
