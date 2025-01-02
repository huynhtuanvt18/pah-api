using AutoMapper;
using DataAccess.Models;
using Respon.TransactionRes;

namespace API.Mapper
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Transaction, TransactionResponse>();
        }
    }
}
