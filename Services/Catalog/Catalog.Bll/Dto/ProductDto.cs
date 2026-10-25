using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Dto
{
    public record ProductDto(long ProductId, string Sku, string Name, decimal Price,
                             int StockQuantity, bool IsActive, long BrandId);
}
