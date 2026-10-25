using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Dto
{
    public record CategoryWithProductsDto(long CategoryId, string Name, IReadOnlyList<ProductDto> Products);
}
