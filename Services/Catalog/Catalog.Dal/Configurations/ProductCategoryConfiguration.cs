using Catalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Configurations
{
    public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.ToTable("ProductCategories");
            builder.HasKey(pc => new { pc.ProductId, pc.CategoryId });    // складений ключ
            builder.Property(pc => pc.AddedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasIndex(pc => pc.ProductId).IsUnique()               // одна основна категорія
                .HasDatabaseName("UX_PC_OnePrimary")
                .HasFilter("[IsPrimary] = 1");
            builder.HasOne(pc => pc.Product).WithMany(p => p.ProductCategories)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(pc => pc.Category).WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
