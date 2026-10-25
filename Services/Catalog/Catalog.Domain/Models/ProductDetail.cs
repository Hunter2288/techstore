using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Models
{
    public class ProductDetail                                
    {
        public long ProductId { get; set; }
        public string? Description { get; set; }
        public int WarrantyMonths { get; set; } = 12;
        public decimal? WeightKg { get; set; }
        public string? Specs { get; set; }                     
    }
}
