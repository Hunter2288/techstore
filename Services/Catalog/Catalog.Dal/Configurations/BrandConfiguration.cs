using Catalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Configurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");
            builder.HasKey(b => b.BrandId);
            builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
            builder.Property(b => b.Country).HasMaxLength(60);
            builder.HasIndex(b => b.Name).IsUnique();
            builder.HasData(
    new { BrandId = 1L, Name = "Samsung", Country = (string?)"South Korea" },
    new { BrandId = 2L, Name = "Lenovo", Country = (string?)"China" },
    new { BrandId = 3L, Name = "Sony", Country = (string?)"Japan" });

        }
    }
}
