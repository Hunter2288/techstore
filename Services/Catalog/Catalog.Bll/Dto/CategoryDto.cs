using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Dto
{
    public record CategoryDto(long CategoryId, string Name, long? ParentCategoryId);
}
