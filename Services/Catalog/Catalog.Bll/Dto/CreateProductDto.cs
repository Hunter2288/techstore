using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Catalog.Bll.Dto
{
    public class CreateProductDto
    {
        [Required, StringLength(200)] public string Name { get; set; } = "";
        [Required, StringLength(40)] public string Sku { get; set; } = "";
        [Range(0.01, 10_000_000)] public decimal Price { get; set; }
        [Range(1, long.MaxValue)] public long BrandId { get; set; }
        [Range(0, 1_000_000)] public int InitialStock { get; set; }
    }
}
