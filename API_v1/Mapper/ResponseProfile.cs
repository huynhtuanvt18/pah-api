using AutoMapper;
using DataAccess.Models;
using Request;

namespace API.Mapper
{
    public class ResponseProfile : Profile
    {
        public ResponseProfile()
        {

            CreateMap<ResponseRequest, DataAccess.Models.Response>();
        }
    }
}
