using API.ErrorHandling;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Request;
using Request.Param;
using Respon;
using Respon.UserRes;
using Service;
using System.Net;
using static Google.Apis.Requests.BatchRequest;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISellerService _sellerService;
        private readonly IMapper _mapper;
        private readonly FirebaseMessaging messaging;

        public UserController(IUserService userService, IMapper mapper, ISellerService sellerService)
        {
            _userService = userService;
            _mapper = mapper;
            _sellerService = sellerService;
            var app = FirebaseApp.DefaultInstance;
            if (FirebaseApp.DefaultInstance == null)
            {
                app = FirebaseApp.Create(new AppOptions() { Credential = GoogleCredential.FromFile("firebase-key.json").CreateScoped("https://www.googleapis.com/auth/firebase.messaging") });
            }
            messaging = FirebaseMessaging.GetMessaging(app);
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpGet("current")]
        public IActionResult Get()
        {
            var userId = GetUserIdFromToken();
            if(userId == -1)
            {
                return Unauthorized(new ErrorDetails
                { 
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn phải đăng nhập để truy cập nội dung này" 
                });
            }
            var user = _userService.Get(userId);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy thông tin người dùng hiện tại thành công", 
                Data = _mapper.Map<UserDetailResponse>(user)
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetByUserId(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == -1)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn phải đăng nhập để truy cập nội dung này"
                });
            }
            var user = _userService.Get(id);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy thông tin người dùng thành công",
                Data = _mapper.Map<UserDetailResponse>(user)
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] PagingParam pagingParam)
        {
            List<User> userList = _userService.GetAll().Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<UserResponse> mappedList = _mapper.Map<List<UserResponse>>(userList);
            UserListCountResponse responses = new UserListCountResponse()
            {
                Count = _userService.GetAll().Count(),
                UserList = mappedList
            };
            return Ok(new BaseResponse 
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách tất cả người dùng thành công", 
                Data = responses
            });
        }

        [HttpGet("customer")]
        public IActionResult GetAllBuyerAndSeller([FromQuery] PagingParam pagingParam)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user.Role != (int)Role.Manager && user.Role != (int)Role.Staff && user.Role != (int)Role.Administrator)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            List<User> userList = _userService.GetAllBuyersSellers()
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList(); 
            int count = _userService.GetAllBuyersSellers().Count();
            List<UserResponse> mappedList = _mapper.Map<List<UserResponse>>(userList);
            CustomerListCountResponse response = new CustomerListCountResponse()
            {
                Count = count,
                CustomerList = mappedList
            };
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách tất cả khách hàng thành công",
                Data = response
            });
        }

        [HttpGet("buyer")]
        public IActionResult GetBuyersByOrders([FromQuery] PagingParam pagingParam)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user == null || user.Role != (int)Role.Manager && user.Role != (int)Role.Staff && user.Role != (int)Role.Administrator)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            List<User> buyerList = _userService.GetBuyersWithDoneOrders();
            List<BuyerWithOrderNumberResponse> mappedList = _mapper.Map<List<BuyerWithOrderNumberResponse>>(buyerList);
            foreach(var item in mappedList)
            {
                item.NumberOfDoneOrders = _userService.CountDoneOrdersByBuyerId(item.Id);
            }
            List<BuyerWithOrderNumberResponse> responses = mappedList.OrderByDescending(b => b.NumberOfDoneOrders).ToList();
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách người mua theo số lượng đơn hàng thành công",
                Data = responses
            });
        }

        [HttpGet("/api/staff")]
        public IActionResult GetAllStaffs([FromQuery] PagingParam pagingParam)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            List<User> staffList = _userService.GetAllStaffs().Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<StaffResponse> mappedList = _mapper.Map<List<StaffResponse>>(staffList);
            StaffListCountResponse response = new StaffListCountResponse()
            {
                Count = _userService.GetAllStaffs().Count(),
                StaffList = mappedList
            };
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách tất cả nhân viên thành công",
                Data = response
            });
        }

        [HttpGet("/api/staff/available")]
        public IActionResult GetAllAvailableStaffs()
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);
            if (user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            List<User> staffList = _userService.GetAvailableStaffs();
            List<StaffResponse> responses = _mapper.Map<List<StaffResponse>>(staffList);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách tất cả nhân viên thành công",
                Data = responses
            });
        }

        [HttpGet("deactivate")]
        public IActionResult SelfDeactivate()
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            _userService.Deactivate(user);
            return Ok(new BaseResponse
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Vô hiệu hóa tài khoản thành công", 
                Data = null
            });
        }

        [HttpGet("reactivate/request")]
        public IActionResult GetReactivateRequestList()
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Staff))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            };
            var reactivateRequests = _userService.GetReactivateRequestList();
            List<UserResponse> responses = _mapper.Map<List<UserResponse>>(reactivateRequests);    
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách yêu cầu tái kích hoạt tài khoản thành công",
                Data = responses
            });
        }

        [HttpGet("deactivate/{id}")]
        public IActionResult Deactivate(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            };
            _userService.Deactivate(_userService.Get(id));
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Vô hiệu hóa thành công",
                Data = null
            });
        }

        [HttpGet("reactivate/{id}")]
        public IActionResult Reactivate(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }            
            _userService.Reactivate(_userService.Get(id));
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Tái kích hoạt tài khoản thành công",
                Data = null
            });
        }

        [HttpGet("seller/approve")]
        public async Task<IActionResult> AcceptSeller(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator)))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            var seller = _sellerService.GetSeller(id);
            _userService.AcceptSeller(seller);
            var notiMessage = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = "Yêu cầu đã được duyệt",
                    Body = "Hệ thống đã duyệt yêu cầu của bạn. Hãy bắt đầu đăng sản phẩm lên PAH!"
                },
                Topic = "USER_" + id
            };
            await messaging.SendAsync(notiMessage);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Chấp nhận yêu cầu trở thành người bán thành công",
                Data = null
            });
        }

        [HttpGet("seller/reject")]
        public async Task<IActionResult> RejectSeller(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator)))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            var seller = _sellerService.GetSeller(id);
            _userService.RejectSeller(seller);
            var notiMessage = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = "Yêu cầu đã bị từ chối",
                    Body = "Hồ sơ của bạn chưa đủ tin cậy để làm người bán. Hãy cập nhật lại thông tin để hệ thống kiểm duyệt lại!"
                },
                Topic = "USER_" + id
            };
            await messaging.SendAsync(notiMessage);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Từ chối yêu cầu trở thành người bán thành công",
                Data = null
            });
        }

        [HttpPost("profile")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult UpdateUser([FromBody] UpdateProfileRequest request) {
            var userId = GetUserIdFromToken();
            _userService.UpdateProfile(userId, request);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Cập nhật thông tin cá nhân thành công",
                Data = null
            });
        }


        [HttpPost("password")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            _userService.ChangePassword(request, user.Email);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Đổi mật khẩu thành công",
                Data = null
            });
        }

        [HttpGet("revenue/{year}")]
        public IActionResult GetPlatformRevenueByMonths(int year)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Manager && user.Role != (int)Role.Staff && user.Role != (int)Role.Administrator))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            var revenueList = _userService.GetRevenues(year);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy doanh thu nền tàng thành công",
                Data = revenueList
            });
        }
    }
}
