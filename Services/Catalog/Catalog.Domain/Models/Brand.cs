using Catalog.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Domain.Models
{
    public class Brand
    {
        public long BrandId { get; private set; }
        public string Name { get; private set; } = "";
        public string? Country { get; private set; }
        public List<Product> Products { get; } = new();

        private Brand() { }                                   // для EF Core
        public Brand(string name, string? country = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Brand name is required");
            Name = name;
            Country = country;
        }
    }
}
