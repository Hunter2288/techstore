using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Dto
{
    public record ProductDetailDto(string? Description, int WarrantyMonths, decimal? WeightKg, string? Specs);
}
