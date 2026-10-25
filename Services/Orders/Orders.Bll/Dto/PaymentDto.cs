using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Bll.Dto
{
    public record PaymentDto(long PaymentId, long OrderId, decimal Amount, string Method,
                             string Status, string? ExternalTxnId, DateTime? PaidAt);
}
