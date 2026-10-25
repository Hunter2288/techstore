using Catalog.Dal.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal
{
    public class UnitOfWork(CatalogDbContext context) : IUnitOfWork
    {
        public IBrandRepository Brands { get; } = new BrandRepository(context);
        public IProductRepository Products { get; } = new ProductRepository(context);
        public ICategoryRepository Categories { get; } = new CategoryRepository(context);
        public IProductCategoryRepository ProductCategories { get; } = new ProductCategoryRepository(context);

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => context.SaveChangesAsync(ct);
    }
}
