using AutoMapper;
using DataAccess.Models;
using Respon.BidderRes;

namespace API.Mapper
{
    public class BidderProfile : Profile
    {
        public BidderProfile()
        {
            CreateMap<Buyer, BidderResponse>();
        }
    }
}
