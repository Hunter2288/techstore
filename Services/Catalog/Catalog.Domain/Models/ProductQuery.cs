using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Models
{
    public record ProductQuery(
        int Page = 1, int PageSize = 20, string? Search = null,
        decimal? MinPrice = null, decimal? MaxPrice = null,
        long? BrandId = null, long? CategoryId = null,
        string SortBy = "name", string SortDir = "asc");
}
