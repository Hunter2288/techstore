using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Product?> GetWithDetailsAsync(long id, CancellationToken ct = default);   // Eager
        Task<bool> ExistsBySkuAsync(string sku, CancellationToken ct = default);
        Task<PagedResult<Product>> GetPagedAsync(ProductQuery query, CancellationToken ct = default);
    }
}
