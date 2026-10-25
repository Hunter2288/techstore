using Catalog.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Services
{

    public interface IProductService
    {
        Task<ProductCardDto> GetByIdAsync(long id, CancellationToken ct);
        Task<ProductDto> CreateAsync(CreateProductDto input, CancellationToken ct);
        Task ChangePriceAsync(long id, decimal newPrice, CancellationToken ct);
        Task RemoveStockAsync(long id, int quantity, CancellationToken ct);
        Task DeleteAsync(long id, CancellationToken ct);
        Task<PagedResult<ProductDto>> GetListAsync(ProductQuery query, CancellationToken ct);
        Task<IReadOnlyList<ProductDto>> GetRecentlyAddedAsync(string category, int days, CancellationToken ct);
    }
}
