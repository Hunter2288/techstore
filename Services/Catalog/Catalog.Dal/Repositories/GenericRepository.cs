using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Repositories
{
    public class GenericRepository<T>(CatalogDbContext context) : IGenericRepository<T> where T : class
    {
        protected readonly CatalogDbContext Context = context;
        protected DbSet<T> Set => Context.Set<T>();

        public virtual async Task<T?> GetByIdAsync(long id, CancellationToken ct = default)
            => await Set.FindAsync(new object[] { id }, ct);

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
            => await Set.AsNoTracking().ToListAsync(ct);

        public async Task AddAsync(T entity, CancellationToken ct = default)
            => await Set.AddAsync(entity, ct);

        public void Update(T entity) => Set.Update(entity);
        public void Delete(T entity) => Set.Remove(entity);


    }

}
