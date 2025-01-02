using AutoMapper;
using DataAccess.Models;
using Request;
using Respon.AddressRes;

namespace API.Mapper {
    public class AddressProfile : Profile{
        public AddressProfile() {
            CreateMap<AddressRequest, Address>();
            CreateMap<Address, AddressResponse>();
        }
    }
}
