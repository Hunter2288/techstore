using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Catalog.Bll.Dto
{
    public class ChangePriceDto
    {
        [Range(0.01, 10_000_000)] public decimal Price { get; set; }
    }
}
