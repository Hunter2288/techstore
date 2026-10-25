using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Orders.Bll.Dto
{
    public class CustomerInputDto
    {
        [Required, StringLength(150)] public string FullName { get; set; } = "";
        [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
        [Phone, StringLength(20)] public string? Phone { get; set; }
    }
}
