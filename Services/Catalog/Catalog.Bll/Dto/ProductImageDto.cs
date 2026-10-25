using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Dto
{
    public record ProductImageDto(long ImageId, string Url, bool IsMain, int SortOrder);
}
