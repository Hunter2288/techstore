using Catalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Configurations
{

    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products", t =>
            {
                t.HasCheckConstraint("CK_Products_Price", "[Price] > 0");
                t.HasCheckConstraint("CK_Products_Stock", "[StockQuantity] >= 0");
            });
            builder.HasKey(p => p.ProductId);
            builder.Property(p => p.Sku).IsRequired().HasMaxLength(40);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.Property(p => p.CreatedBy).HasMaxLength(100);
            builder.Property(p => p.UpdatedBy).HasMaxLength(100);
            builder.Property(p => p.RowVersion).IsRowVersion();

            builder.HasIndex(p => p.Sku).IsUnique();
            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.Price)                                // сторінка каталогу
                .HasDatabaseName("IX_Products_Active_Price")
                .IncludeProperties(p => p.Name)
                .HasFilter("[IsActive] = 1 AND [IsDeleted] = 0");

            builder.HasOne(p => p.Brand).WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(p => p.Detail).WithOne()                       // 1:1
                .HasForeignKey<ProductDetail>(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(p => p.Images).WithOne()                      // 1:N
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
