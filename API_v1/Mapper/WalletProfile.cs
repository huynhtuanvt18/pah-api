using AutoMapper;
using DataAccess.Models;
using Respon.WalletRes;

namespace API.Mapper
{
    public class WalletProfile : Profile
    {
        public WalletProfile() 
        { 
            CreateMap<Wallet, WalletCurrentUserResponse>();
            CreateMap<Withdrawal, WithdrawalResponse>();
        }
    }
}
