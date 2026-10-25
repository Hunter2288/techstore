using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Interfaces
{
    public interface IUnitOfWork
    {
        IBrandRepository Brands { get; }
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IProductCategoryRepository ProductCategories { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
