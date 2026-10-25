using Ardalis.Specification;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Catalog.Dal.Specifications
{
    public class ProductsFilterSpec : Specification<Product>
    {
        public ProductsFilterSpec(ProductQuery q)
        {
            Query.Where(p => p.IsActive && !p.IsDeleted);
            if (!string.IsNullOrWhiteSpace(q.Search))
                Query.Where(p => p.Name.Contains(q.Search));
            if (q.MinPrice is not null)
                Query.Where(p => p.Price >= q.MinPrice.Value);
            if (q.MaxPrice is not null)
                Query.Where(p => p.Price <= q.MaxPrice.Value);
            if (q.BrandId is not null)
                Query.Where(p => p.BrandId == q.BrandId.Value);
            if (q.CategoryId is not null)
                Query.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == q.CategoryId.Value));
        }
    }
}
