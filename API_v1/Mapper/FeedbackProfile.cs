using AutoMapper;
using DataAccess.Models;
using Request;
using Respon.FeedbackRes;

namespace API.Mapper
{
    public class FeedbackProfile : Profile
    {
        public FeedbackProfile()
        {
            CreateMap<FeedbackRequest, Feedback>();
            CreateMap<Feedback, FeedbackResponse>();
        }
    }
}
