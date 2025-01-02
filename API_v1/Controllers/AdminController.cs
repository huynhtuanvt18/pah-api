using API.ErrorHandling;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Request;
using Request.Param;
using Respon;
using Respon.UserRes;
using Service;
using Service.EmailService;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase {
        private readonly IMapper _mapper;
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly string _templatesPath;

        public AdminController(IMapper mapper, IAdminService adminService, IUserService userService, IEmailService emailService, IConfiguration configuration) {
            _mapper = mapper;
            _adminService = adminService;
            _userService = userService;
            _emailService = emailService;
            _templatesPath = configuration["Path:Templates"];
        }

        private int GetUserIdFromToken() {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpPost("staff")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<IActionResult> AddStaffAsync([FromBody] StaffRequest request) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không được truy cập tính năng này"
                });
            }
            if (user.Role != (int) Role.Administrator) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không được truy cập tính năng này"
                });
            }
            _adminService.CreateStaff(_mapper.Map<User>(request));

            var code = _userService.CreateVerificationCode(request.Email);
            var link = Url.Link("Verify account", new { email = request.Email, code = code });

            // Get HTML template
            string fullPath = Path.Combine(_templatesPath, "RegisterEmail.html");
            StreamReader str = new StreamReader(fullPath);
            string mailText = str.ReadToEnd();
            str.Close();
            mailText = mailText.Replace("[verifyLink]", link);

            var message = new Message(new string[] { request.Email }, "Xác thực tài khoản nhân viên PAH", mailText);
            await _emailService.SendEmail(message);
            return Ok(new BaseResponse {
                Code = 200,
                Message =
                "Tạo tài khoản nhân viên thành công. Nhân viên cần phải xác thực email để có thể đăng nhập!",
                Data = null
            });
        }
        
        [HttpPatch("staff")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult EditStaff([FromBody] StaffRequest request) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không được truy cập tính năng này"
                });
            }
            if (user.Role != (int) Role.Administrator) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không được truy cập tính năng này"
                });
            }
            _adminService.UpdateStaff(_mapper.Map<User>(request));
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Cập nhật thông tin nhân viên thành công",
                Data = null
            });
        }

        [HttpGet("account")]
        public IActionResult ViewAllAccount([FromQuery] AccountParam accountParam, [FromQuery] PagingParam pagingParam) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không được truy cập tính năng này"
                });
            }
            if (user.Role != (int) Role.Administrator) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không được truy cập tính năng này"
                });
            }
            var list = _adminService.GetAccounts(accountParam);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Get all accounts successfully",
                Data = new { 
                    Count = list.Count,
                    List = list.Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).Select(p => _mapper.Map<UserResponse>(p)).ToList()
                }
            });
        }
        
        [HttpPatch("account")]
        public IActionResult EditStatusAccount([FromBody] AccountUpdate request) {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không được truy cập tính năng này"
                });
            }
            if (user.Role != (int) Role.Administrator) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không được truy cập tính năng này"
                });
            }
            _adminService.UpdateStatusAccount(request.Id, request.Status);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Update account status successfully",
                Data = null
            });
        }

        public class AccountUpdate {
            [Required]
            public int Id { get; set; }
            [Required]
            [Range(0, 1)]
            public int Status { get; set; }
        }
    }
}
