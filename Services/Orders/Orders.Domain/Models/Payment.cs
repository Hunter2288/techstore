using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Domain.Models
{
    public class Payment
    {
        public long PaymentId { get; set; }
        public long OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public string? ExternalTxnId { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
