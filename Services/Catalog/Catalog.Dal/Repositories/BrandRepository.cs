using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Repositories
{
    public class BrandRepository(CatalogDbContext context)
        : GenericRepository<Brand>(context), IBrandRepository;

}
