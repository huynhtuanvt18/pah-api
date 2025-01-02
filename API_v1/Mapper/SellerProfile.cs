using AutoMapper;
using DataAccess.Models;
using Request;
using Respon.SellerRes;

namespace API.Mapper
{
    public class SellerProfile : Profile
    {
        public SellerProfile() 
        {
            CreateMap<SellerRequest, Seller>();
            CreateMap<Seller, SellerWithAddressResponse>();
            CreateMap<Seller, SellerDetailResponse>();
            CreateMap<Seller, SellerRequestResponse>();
            CreateMap<Seller, SellerWithSalesResponse>();
        }
    }
}
