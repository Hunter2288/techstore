using Catalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Configurations
{
    public class ProductDetailConfiguration : IEntityTypeConfiguration<ProductDetail>
    {
        public void Configure(EntityTypeBuilder<ProductDetail> builder)
        {
            builder.ToTable("ProductDetails", t =>
            {
                t.HasCheckConstraint("CK_Details_Warranty", "[WarrantyMonths] BETWEEN 0 AND 120");
                t.HasCheckConstraint("CK_Details_Weight", "[WeightKg] > 0");
                t.HasCheckConstraint("CK_Details_Specs", "[Specs] IS NULL OR ISJSON([Specs]) = 1");
            });
            builder.HasKey(d => d.ProductId);
            builder.Property(d => d.ProductId).ValueGeneratedNever();
            builder.Property(d => d.WeightKg).HasColumnType("decimal(6,2)");
        }
    }

}
