using AutoMapper;
using DataAccess.Models;
using Respon.BidRes;

namespace API.Mapper
{
    public class BidProfile : Profile
    {
        public BidProfile() 
        {
            CreateMap<Bid, BidResponse>();
            CreateMap<Request.BidRequest, Bid>();
        }
    }
}
