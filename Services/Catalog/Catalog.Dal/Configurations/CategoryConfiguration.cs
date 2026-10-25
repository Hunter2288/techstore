using Catalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Dal.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(c => c.CategoryId);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(c => c.Name).IsUnique();
            builder.HasOne(c => c.Parent).WithMany(c => c.Children)      
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasData(
    new { CategoryId = 1L, Name = "Електроніка", ParentCategoryId = (long?)null },
    new { CategoryId = 2L, Name = "Смартфони", ParentCategoryId = (long?)1L },
    new { CategoryId = 3L, Name = "Ноутбуки", ParentCategoryId = (long?)1L },
    new { CategoryId = 4L, Name = "Аудіо", ParentCategoryId = (long?)1L });
        }
    }
}
