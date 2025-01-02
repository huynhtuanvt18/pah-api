using AutoMapper;
using DataAccess.Models;
using Request;
using Respon.AuctionRes;

namespace API.Mapper
{
    public class AuctionProfile : Profile
    {
        public AuctionProfile() 
        {
            CreateMap<AuctionRequest, Auction>();
            CreateMap<Auction, AuctionListResponse>();
            CreateMap<Auction, AuctionListBidderResponse>();
            CreateMap<Auction, AuctionDetailResponse>();
            CreateMap<Auction, AuctionResponse>();
            CreateMap<Auction, AuctionListEndedResponse>();
        }
    }
}
