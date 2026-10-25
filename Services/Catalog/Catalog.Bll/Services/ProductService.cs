using Catalog.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Services
{
    public class ProductService(IUnitOfWork uow, IMapper mapper) : IProductService
    {
        public async Task<ProductCardDto> GetByIdAsync(long id, CancellationToken ct)
            => mapper.Map<ProductCardDto>(await uow.Products.GetWithDetailsAsync(id, ct)
                ?? throw new EntityNotFoundException($"Product {id} not found"));

        public async Task<ProductDto> CreateAsync(CreateProductDto input, CancellationToken ct)
        {
            if (await uow.Products.ExistsBySkuAsync(input.Sku, ct))
                throw new ConflictException($"Product with SKU '{input.Sku}' already exists");
            _ = await uow.Brands.GetByIdAsync(input.BrandId, ct)
                ?? throw new EntityNotFoundException($"Brand {input.BrandId} not found");

            var product = new Product(input.Name, input.Sku, input.Price, input.BrandId, "api");
            if (input.InitialStock > 0) product.AddStock(input.InitialStock, "api");

            await uow.Products.AddAsync(product, ct);
            await uow.SaveChangesAsync(ct);
            return mapper.Map<ProductDto>(product);
        }

        public async Task ChangePriceAsync(long id, decimal newPrice, CancellationToken ct)
        {
            var product = await GetActiveAsync(id, ct);
            product.ChangePrice(newPrice, "api");           // правило "ціна > 0" перевіряє сутність
            await uow.SaveChangesAsync(ct);
        }

        public async Task RemoveStockAsync(long id, int quantity, CancellationToken ct)
        {
            var product = await GetActiveAsync(id, ct);
            product.RemoveStock(quantity, "api");           // нестача залишку -> DomainException
            await uow.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(long id, CancellationToken ct)
        {
            var product = await GetActiveAsync(id, ct);
            product.SoftDelete("api");
            await uow.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<ProductDto>> GetRecentlyAddedAsync(string category, int days, CancellationToken ct)
        {
            var since = DateTime.UtcNow.AddDays(-days);
            return mapper.Map<List<ProductDto>>(
                await uow.ProductCategories.GetRecentlyAddedAsync(category, since, ct));
        }

        private async Task<Product> GetActiveAsync(long id, CancellationToken ct)
        {
            var product = await uow.Products.GetByIdAsync(id, ct);
            return product is null || product.IsDeleted
                ? throw new EntityNotFoundException($"Product {id} not found")
                : product;
        }

        public async Task<PagedResult<ProductDto>> GetListAsync(ProductQuery q, CancellationToken ct)
        {
            var sortBy = (q.SortBy ?? "").ToLowerInvariant();
            var sortDir = (q.SortDir ?? "").ToLowerInvariant();
            if (sortBy is not ("name" or "price" or "stock"))
                throw new ValidationException("sortBy must be one of: name, price, stock");
            if (sortDir is not ("asc" or "desc"))
                throw new ValidationException("sortDir must be 'asc' or 'desc'");
            if (q.MinPrice > q.MaxPrice)
                throw new ValidationException("minPrice must not exceed maxPrice");

            q = q with
            {
                Page = Math.Max(q.Page, 1),
                PageSize = Math.Clamp(q.PageSize, 1, 100),
                SortBy = sortBy,
                SortDir = sortDir
            };

            var page = await uow.Products.GetPagedAsync(q, ct);
            return new PagedResult<ProductDto>(
                mapper.Map<List<ProductDto>>(page.Items), page.TotalCount, page.Page, page.PageSize);
        }

    }
}
