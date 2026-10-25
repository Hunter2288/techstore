using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Dto
{
    public record ProductCardDto(long ProductId, string Sku, string Name, decimal Price, int StockQuantity,
                                 string BrandName, ProductDetailDto? Detail,
                                 IReadOnlyList<ProductImageDto> Images,
                                 IReadOnlyList<ProductCategoryDto> Categories);
}
