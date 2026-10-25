using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;

namespace Catalog.Dal.Specifications
{
    public class ProductsPageSpec : ProductsFilterSpec
    {
        public ProductsPageSpec(ProductQuery q) : base(q)
        {
            var desc = q.SortDir == "desc";
            switch (q.SortBy)
            {
                case "price":
                    if (desc) Query.OrderByDescending(p => p.Price).ThenBy(p => p.ProductId);
                    else Query.OrderBy(p => p.Price).ThenBy(p => p.ProductId);
                    break;
                case "stock":
                    if (desc) Query.OrderByDescending(p => p.StockQuantity).ThenBy(p => p.ProductId);
                    else Query.OrderBy(p => p.StockQuantity).ThenBy(p => p.ProductId);
                    break;
                default:
                    if (desc) Query.OrderByDescending(p => p.Name).ThenBy(p => p.ProductId);
                    else Query.OrderBy(p => p.Name).ThenBy(p => p.ProductId);
                    break;
            }
            Query.Skip((q.Page - 1) * q.PageSize).Take(q.PageSize);
        }
    }
}
