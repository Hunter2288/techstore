using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Interfaces
{
    public interface IProductCategoryRepository : IGenericRepository<ProductCategory>
    {
        Task<IReadOnlyList<Product>> GetRecentlyAddedAsync(                            // LINQ, M:N
            string categoryName, DateTime since, CancellationToken ct = default);
    }
}
