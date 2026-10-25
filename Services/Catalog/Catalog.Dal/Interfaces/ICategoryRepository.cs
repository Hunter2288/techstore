using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetWithProductsAsync(long id, CancellationToken ct = default); // Explicit
    }
}
