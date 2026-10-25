using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Models
{
    public class ProductCategory                               
    {
        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public long CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
