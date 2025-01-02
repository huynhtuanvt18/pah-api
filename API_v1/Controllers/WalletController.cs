using API.ErrorHandling;
using Microsoft.AspNetCore.Cors;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Request.ThirdParty.Zalopay;
using System.Net;
using DataAccess;
using Respon;
using Respon.WalletRes;
using Request;
using Request.Param;
using Respon.OrderRes;
using System.Collections.Generic;
using Respon.AuctionRes;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableCors]
    public class WalletController : ControllerBase {
        private readonly IWalletService _walletService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public WalletController(IWalletService walletService, IUserService userService, IMapper mapper) {
            _walletService = walletService;
            _userService = userService;
            _mapper = mapper;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpPost("topup")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<IActionResult> Topup([FromBody] TopupRequest request) {
            var userId = GetUserIdFromToken();  
            if (userId == null) {
                return Unauthorized(new ErrorDetails { 
                    StatusCode = (int) HttpStatusCode.Unauthorized, 
                    Message = "Bạn phải đăng nhập để truy cập nội dung này" 
                });
            }

            var user = _userService.Get(userId);
            if (user == null) {
                return Unauthorized(new ErrorDetails { 
                    StatusCode = (int) HttpStatusCode.Unauthorized, 
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }

            await _walletService.Topup(userId, request);
            return Ok(new BaseResponse { 
                Code = (int) HttpStatusCode.OK, 
                Message = "Nạp tiền thành công", 
                Data = null 
            });
        }

        [HttpPost("payment/order/{orderId:int}")]
        public IActionResult Pay(int orderId) {
            var userId = GetUserIdFromToken();
            if (userId == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn phải đăng nhập để truy cập nội dung này"
                });
            }
            var user = _userService.Get(userId);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Tài khoản của bạn đang không khả dụng"
                });
            }
            if (user.Role != (int) Role.Buyer && user.Role != (int) Role.Seller) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            _walletService.CheckoutWallet(userId, orderId, (int) OrderStatus.WaitingSellerConfirm);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = $"Thanh toán đơn hàng {orderId} thành công",
                Data = null
            });
        }

        [HttpGet("current")]
        public IActionResult GetByCurrentUser()
        {
            var userId = GetUserIdFromToken();
            Wallet wallet = _walletService.GetByCurrentUser(userId);
            if (wallet == null)
            {
                wallet = new Wallet();
            }
            WalletCurrentUserResponse response = _mapper.Map<WalletCurrentUserResponse>(wallet);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy thông tin ví của người dùng hiện tại thành công",
                Data = response
            });
        }

        [HttpPost("withdraw")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult Withdrawal([FromBody] WithdrawalRequest request) {
            var userId = GetUserIdFromToken();
            if (userId == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn phải đăng nhập để truy cập nội dung này"
                });
            }

            var user = _userService.Get(userId);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            if (user.Role != (int) Role.Seller && user.Role != (int) Role.Buyer) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            _walletService.CreateWithdrawal(userId, request);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Tạo yêu cầu rút tiền thành công",
                Data = null
            });
        }
        
        [HttpGet("withdraw")]
        public IActionResult GetWithdrawal([FromQuery] PagingParam pagingParam) {
            var userId = GetUserIdFromToken();
            if (userId == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn phải đăng nhập để truy cập nội dung này"
                });
            }

            var user = _userService.Get(userId);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            
            var data = _walletService.GetWithdrawalByUserId(userId).Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize)
                .Take(pagingParam.PageSize).ToList();
            List<WithdrawalResponse> mappedList = _mapper.Map<List<WithdrawalResponse>>(data);

            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Lấy yêu cầu rút tiền thành công",
                Data = mappedList
            });
        }

        [HttpGet("manager/withdraw")]
        public IActionResult GetWithdrawalManager([FromQuery] PagingParam pagingParam)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn phải đăng nhập để truy cập nội dung này"
                });
            }

            var user = _userService.Get(userId);
            if (user == null)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            if (user.Role != (int)Role.Manager)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            var data = _walletService.GetWithdrawalManager().Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize)
                .Take(pagingParam.PageSize).ToList();
            int count = _walletService.GetWithdrawalManager().Count;
            List<WithdrawalResponse> mappedList = _mapper.Map<List<WithdrawalResponse>>(data);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách yêu cầu rút tiền thành công",
                Data = new
                {
                    Count = count,
                    List = mappedList
                }
            });
        }

        [HttpPost("manager/withdraw")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult UpdateWithdrawal([FromBody] UpdateWithdrawRequest request) {
            var userId = GetUserIdFromToken();
            if (userId == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn phải đăng nhập để truy cập nội dung này"
                });
            }

            var user = _userService.Get(userId);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            if (user.Role != (int) Role.Manager) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            if (request.Status == (int) WithdrawalStatus.Done) {
                _walletService.ApproveWithdrawal(request.WithdrawalId, userId);
            } else {
                _walletService.DenyWithdrawal(request.WithdrawalId, userId);
            }
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Cập nhật yêu cầu rút tiền thành công",
                Data = null
            });
        }
    }
}
