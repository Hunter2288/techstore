using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Models
{
    public class ProductImage                                 
    {
        public long ImageId { get; set; }
        public long ProductId { get; set; }
        public string Url { get; set; } = "";
        public bool IsMain { get; set; }
        public int SortOrder { get; set; }
    }
}
