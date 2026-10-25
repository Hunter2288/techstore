using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Orders.Bll.Dto
{
    public class RegisterPaymentDto
    {
        [Range(0.01, 999999999)] public decimal Amount { get; set; }
        [Required, RegularExpression("^(Card|Cash|Transfer)$")] public string Method { get; set; } = "";
        [StringLength(100)] public string? ExternalTxnId { get; set; }
    }
}
