using AutoMapper;
using DataAccess.Models;
using Request;
using Respon.ProductRes;

namespace API.Mapper
{
    public class ProductProfile : Profile
    {
        public ProductProfile() 
        {
            CreateMap<ProductRequest, Product>();
            CreateMap<Product, ProductListResponse>();
            CreateMap<Product, ProductDetailResponse>();
            CreateMap<Product, ProductResponse>();
        }
    }
}
