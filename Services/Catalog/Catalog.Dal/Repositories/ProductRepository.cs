using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification.EntityFrameworkCore;
using Catalog.Dal.Specifications;

namespace Catalog.Dal.Repositories
{
    public class ProductRepository(CatalogDbContext context)
        : GenericRepository<Product>(context), IProductRepository
    {
        
        public Task<Product?> GetWithDetailsAsync(long id, CancellationToken ct = default)
            => Set.Include(p => p.Brand)
                  .Include(p => p.Detail)
                  .Include(p => p.Images)
                  .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                  .AsNoTracking()
                  .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted, ct);

       
        public Task<bool> ExistsBySkuAsync(string sku, CancellationToken ct = default)
            => Set.AnyAsync(p => p.Sku == sku, ct);

        public async Task<PagedResult<Product>> GetPagedAsync(ProductQuery q, CancellationToken ct = default)
        {
            var total = await Set.WithSpecification(new ProductsFilterSpec(q)).CountAsync(ct);
            var items = await Set.WithSpecification(new ProductsPageSpec(q)).AsNoTracking().ToListAsync(ct);
            return new PagedResult<Product>(items, total, q.Page, q.PageSize);
        }
    }
}
