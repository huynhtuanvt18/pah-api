using API.ErrorHandling;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Request;
using Respon;
using Service;
using Service.Implement;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class ResponseController : ControllerBase
    {
        private readonly IResponseService _responseService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ResponseController(IResponseService responseService, IMapper mapper, IUserService userService)
        {
            _responseService = responseService;
            _mapper = mapper;
            _userService = userService;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpGet]
        public IActionResult Get(int feedbackId)
        {
            DataAccess.Models.Response response = _responseService.GetByFeedbackId(feedbackId);
            return Ok(new BaseResponse 
            {
                Code = (int)HttpStatusCode.OK, 
                Message = "Lấy phản hồi đánh giá thành công",
                Data = response 
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] ResponseRequest request)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _responseService.Reply(_mapper.Map<DataAccess.Models.Response>(request));
            return Ok(new BaseResponse
            { 
                Code = (int)HttpStatusCode.OK,
                Message = "Phản hồi đánh giá thành công", 
                Data = null 
            });
        }
    }
}
