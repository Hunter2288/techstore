using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Repositories
{
    public class ProductCategoryRepository(CatalogDbContext context)
        : GenericRepository<ProductCategory>(context), IProductCategoryRepository
    {
        
        public override Task<ProductCategory?> GetByIdAsync(long id, CancellationToken ct = default)
            => throw new NotSupportedException("ProductCategory has a composite key");

        
        public async Task<IReadOnlyList<Product>> GetRecentlyAddedAsync(
            string categoryName, DateTime since, CancellationToken ct = default)
            => await Set
                .Where(pc => pc.Category.Name == categoryName && pc.AddedAt >= since && !pc.Product.IsDeleted)
                .OrderByDescending(pc => pc.AddedAt)
                .Select(pc => pc.Product)
                .AsNoTracking()
                .ToListAsync(ct);
    }
}
