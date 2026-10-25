using Catalog.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Services
{
    public class CategoryService(IUnitOfWork uow, IMapper mapper) : ICategoryService
    {
        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct)
            => mapper.Map<List<CategoryDto>>(await uow.Categories.GetAllAsync(ct));

        public async Task<CategoryWithProductsDto> GetWithProductsAsync(long id, CancellationToken ct)
            => mapper.Map<CategoryWithProductsDto>(await uow.Categories.GetWithProductsAsync(id, ct)
                ?? throw new EntityNotFoundException($"Category {id} not found"));
    }
}
