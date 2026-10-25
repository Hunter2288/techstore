using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Dto
{
    public record ProductCategoryDto(long CategoryId, string CategoryName, bool IsPrimary);
}
