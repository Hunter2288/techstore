using Catalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Configurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");
            builder.HasKey(i => i.ImageId);
            builder.Property(i => i.Url).IsRequired().HasMaxLength(500);
            builder.HasIndex(i => i.ProductId).IsUnique()                 // одне головне фото
                .HasDatabaseName("UX_Images_OneMain")
                .HasFilter("[IsMain] = 1");
        }
    }
}
