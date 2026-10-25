using Catalog.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Services
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct);
        Task<CategoryWithProductsDto> GetWithProductsAsync(long id, CancellationToken ct);
    }
}
