using AutoMapper;
using BLL.DTOs;
using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product mappings
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null));
            CreateMap<CreateProductDTO, Product>();
            CreateMap<UpdateProductDTO, Product>();
            CreateMap<Product, UpdateProductDTO>();

            // Category mappings
            CreateMap<Categories, CategoryDTO>();
            CreateMap<CreateCategoryDTO, Categories>();
            CreateMap<UpdateCategoryDTO, Categories>();
            CreateMap<Categories, UpdateCategoryDTO>();
            CreateMap<TrackingOrderDTO, TracingOrder>()
                .ForMember(dest => dest.MemberId, opt => opt.Ignore()) // Nếu Id được tự động tạo bởi CSDL
                .ReverseMap();
        }
    }
} 