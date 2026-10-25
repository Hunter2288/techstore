using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Catalog.Bll.Dto
{
    public class RemoveStockDto
    {
        [Range(1, 1_000_000)] public int Quantity { get; set; }
    }
}
