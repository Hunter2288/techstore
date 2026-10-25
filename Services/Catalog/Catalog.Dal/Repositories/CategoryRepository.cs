using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Repositories
{
    public class CategoryRepository(CatalogDbContext context)
        : GenericRepository<Category>(context), ICategoryRepository
    {
        
        public async Task<Category?> GetWithProductsAsync(long id, CancellationToken ct = default)
        {
            var category = await Set.FirstOrDefaultAsync(c => c.CategoryId == id, ct);   // 1-й запит
            if (category is null) return null;

            await Context.Entry(category).Collection(c => c.ProductCategories)
                .Query().Include(pc => pc.Product)
                .LoadAsync(ct);                                                            // 2-й запит
            return category;
        }
    }
}
