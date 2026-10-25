using Catalog.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Models
{
    public class Category
    {
        public long CategoryId { get; private set; }
        public string Name { get; private set; } = "";
        public long? ParentCategoryId { get; private set; }
        public Category? Parent { get; private set; }
        public List<Category> Children { get; } = new();
        public List<ProductCategory> ProductCategories { get; } = new();

        private Category() { }
        public Category(string name, long? parentCategoryId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Category name is required");
            Name = name;
            ParentCategoryId = parentCategoryId;
        }
    }
}
