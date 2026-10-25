using Catalog.Bll.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Bll.Mapping
{
    public class CatalogProfile : Profile
    {
        public CatalogProfile()
        {
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDetail, ProductDetailDto>();
            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<ProductCategory, ProductCategoryDto>();      // CategoryName <- Category.Name
            CreateMap<Product, ProductCardDto>()                   // BrandName <- Brand.Name
                .ForCtorParam("Categories", o => o.MapFrom(s => s.ProductCategories));

            CreateMap<Category, CategoryDto>();
            CreateMap<Category, CategoryWithProductsDto>()
                .ForCtorParam("Products", o => o.MapFrom(s => s.ProductCategories.Select(pc => pc.Product)));
        }
    }
}
