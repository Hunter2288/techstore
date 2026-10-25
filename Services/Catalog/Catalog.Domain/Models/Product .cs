using Catalog.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Models
{
    public class Product                                       // аналог CatalogItem з eShop
    {
        public long ProductId { get; private set; }
        public string Sku { get; private set; } = "";
        public string Name { get; private set; } = "";
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }
        public bool IsActive { get; private set; } = true;
        public long BrandId { get; private set; }
        public Brand Brand { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public string? CreatedBy { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public string? UpdatedBy { get; private set; }
        public bool IsDeleted { get; private set; }
        public byte[] RowVersion { get; private set; } = [];

        public ProductDetail? Detail { get; private set; }                 // 1:1
        public List<ProductImage> Images { get; } = new();                 // 1:N
        public List<ProductCategory> ProductCategories { get; } = new();   // M:N

        private Product() { }
        public Product(string name, string sku, decimal price, long brandId, string? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required");
            if (string.IsNullOrWhiteSpace(sku)) throw new DomainException("SKU is required");
            if (price <= 0) throw new DomainException("Price must be positive");
            Name = name;
            Sku = sku;
            Price = price;
            BrandId = brandId;
            CreatedBy = createdBy;
            CreatedAt = UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePrice(decimal newPrice, string? changedBy = null)
        {
            if (newPrice <= 0) throw new DomainException("Price must be positive");
            Price = newPrice;
            Touch(changedBy);
        }

        public void AddStock(int quantity, string? changedBy = null)
        {
            if (quantity <= 0) throw new DomainException("Quantity must be positive");
            StockQuantity += quantity;
            Touch(changedBy);
        }

        public void RemoveStock(int quantity, string? changedBy = null)
        {
            if (quantity <= 0) throw new DomainException("Quantity must be positive");
            if (StockQuantity < quantity) throw new DomainException($"Only {StockQuantity} units available");
            StockQuantity -= quantity;
            Touch(changedBy);
        }

        public void SetDetail(ProductDetail detail) => Detail = detail;

        public void SoftDelete(string? changedBy = null)
        {
            IsDeleted = true;
            IsActive = false;
            Touch(changedBy);
        }

        private void Touch(string? by)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = by;
        }
    }
}
