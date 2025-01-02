using AutoMapper;
using DataAccess.Models;
using Request;
using Respon.UserRes;

namespace API.Mapper {
    public class UserProfile : Profile{
        public UserProfile() {
            CreateMap<RegisterRequest, User>().ForMember(dest => dest.Name, opt => opt.MapFrom(p => p.Name));
            CreateMap<StaffRequest, User>();
            CreateMap<User, UserResponse>();
            CreateMap<Seller, SellerResponse>();
            CreateMap<User, UserDetailResponse>();
            CreateMap<User, StaffResponse>();
            CreateMap<User, WinnerResponse>();
            CreateMap<User, BuyerWithOrderNumberResponse>();
        }
    }
}
