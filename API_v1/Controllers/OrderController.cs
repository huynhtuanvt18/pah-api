using API.ErrorHandling;
using AutoMapper;
using AutoMapper.Configuration.Conventions;
using DataAccess;
using DataAccess.Models;
using Firebase.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Hangfire.Annotations;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using Request;
using Request.Param;
using Request.ThirdParty.GHN;
using Respon;
using Respon.OrderRes;
using Service;
using Service.EmailService;
using System;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableCors]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IOrderCancelService _orderCancelService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly string _templatesPath;
        private readonly FirebaseMessaging messaging;

        public OrderController(IOrderService orderService, IUserService userService, IMapper mapper,
            IEmailService emailService, IConfiguration pathConfig, IOrderCancelService orderCancelService)
        {
            _orderService = orderService;
            _userService = userService;
            _mapper = mapper;
            _emailService = emailService;
            _orderCancelService = orderCancelService;
            _templatesPath = pathConfig["Path:Templates"];
            var app = FirebaseApp.DefaultInstance;
            if (FirebaseApp.DefaultInstance == null)
            {
                app = FirebaseApp.Create(new AppOptions() { Credential = GoogleCredential.FromFile("firebase-key.json").CreateScoped("https://www.googleapis.com/auth/firebase.messaging") });
            }
            messaging = FirebaseMessaging.GetMessaging(app);
        }

        [HttpGet]
        public IActionResult Get([FromQuery] PagingParam pagingParam, [FromQuery] int Status)
        {
            var list = _orderService.GetAll(Status);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách đơn hàng thành công",
                Data = new
                {
                    Count = list.Count,
                    List = list.Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList()
                            .Select(p => _mapper.Map<OrderResponse>(p))
                }
            });
        }

        [HttpGet("{orderId:int}")]
        public IActionResult Get(int orderId)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn chưa đăng nhập để truy cập nội dung này"
                });
            }

            var order = _orderService.Get(orderId);
            if (order == null)
            {
                return NotFound(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Không tìm thấy đơn hàng"
                });
            }
            OrderDetailResponse response = _mapper.Map<OrderDetailResponse>(order);
            var orderCancel = _orderCancelService.Get(orderId);
            if (orderCancel != null)
            {
                response.OrderCancellation = _mapper.Map<OrderCancellationResponse>(orderCancel);
            } 
            else
            {
                response.OrderCancellation = null;
            }
            //if (order.BuyerId != id && order.SellerId != id) {
            //    return Unauthorized(new ErrorDetails {
            //        StatusCode = (int) HttpStatusCode.Unauthorized,
            //        Message = "You are not allowed to access this order"
            //    });
            //}
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy đơn hàng thành công",
                Data = response
            });
        }

        [HttpGet("/api/buyer/order")]
        public IActionResult GetByBuyerId([FromQuery] PagingParam pagingParam, [FromQuery] OrderParam orderParam)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "Bạn không có quyền truy cập nội dung này" });
            }

            var orders = _orderService.GetByBuyerId(id, orderParam.Status)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            var responseOrders = orders.Select(p => _mapper.Map<OrderResponse>(p));
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Lấy danh sách đơn hàng của người mua thành công", Data = responseOrders });
        }

        [HttpGet("/api/seller/order")]
        public IActionResult GetBySellerId([FromQuery] PagingParam pagingParam, [FromQuery] OrderParam orderParam)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "Bạn không có quyền truy cập nội dung này" });
            }

            var orders = _orderService.GetBySellerId(id, orderParam.Status)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            var responseOrders = orders.Select(p => _mapper.Map<OrderResponse>(p));
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Lấy danh sách đơn hàng của người bán thành công", Data = responseOrders });
        }

        [HttpPost("/api/buyer/checkout")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }

            if (user.Role != (int)Role.Buyer && user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            _orderService.Checkout(request, user.Id, request.AddressId);
            var notiMessage = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = "Đơn hàng mới",
                    Body = "Bạn có đơn hàng mới cần được xác nhận!"
                },
                Data = new Dictionary<string, string>()
                {
                    ["route"] = "SellerOrderList",
                },
                Topic = "USER_" + request.Order.First().SellerId
            };
            await messaging.SendAsync(notiMessage);
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Tạo đơn hàng thành công", Data = null });
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpPost("/api/seller/order/cancelrequest/{orderId:int}")]
        public async Task<IActionResult> ApproveCancelRequestAsync(int orderId)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "Bạn không có quyền truy cập nội dung này" });
            }

            if (user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "Bạn không có quyền truy cập nội dung này" });
            }
            _orderService.ApproveCancelOrderRequest(id, orderId);

            // Get order
            var order = _orderService.Get(orderId);

            // Get Buyer
            var buyer = _userService.Get((int)order.BuyerId);

            // Get HTML template
            string fullPath = Path.Combine(_templatesPath, "CancelApproveEmail.html");
            StreamReader str = new StreamReader(fullPath);
            string mailText = str.ReadToEnd();
            str.Close();
            mailText = mailText.Replace("[orderId]", orderId.ToString())
                .Replace("[shippingAddress]", order.RecipientAddress)
                .Replace("[totalAmount]", order.TotalAmount.ToString())
                .Replace("[shippingCost]", order.ShippingCost.ToString())
                .Replace("[sumAmount]", (order.TotalAmount + order.ShippingCost).ToString());

            var message = new Service.EmailService.Message(new string[] { user.Email, buyer.Email }, $"Yêu cầu hủy đơn #{orderId} được chấp nhận", mailText);
            await _emailService.SendEmail(message);
            var notiMessage = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = "Đơn hàng #" + orderId,
                    Body = "Đơn hàng của bạn đã bị hủy!"
                },
                Data = new Dictionary<string, string>()
                {
                    ["route"] = "BuyerOrderList"
                },
                Topic = "USER_" + buyer.Id
            };
            await messaging.SendAsync(notiMessage);
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Chấp nhận yêu cầu hủy đơn hàng thành công", Data = null });
        }

        [HttpPost("/api/buyer/order/cancelrequest/{orderId:int}")]
        public async Task<IActionResult> CreateCancelRequest(int orderId)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "Bạn không có quyền truy cập nội dung này" });
            }

            if (user.Role != (int)Role.Buyer && user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "Bạn không có quyền truy cập nội dung này" });
            }

            _orderService.CancelOrderRequest(id, orderId);
            var order = _orderService.Get(orderId);
            var seller = _userService.Get((int)order.SellerId);

            var notiMessage = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = "Đơn hàng #" + orderId,
                    Body = "Người mua đã yêu cầu hủy đơn này!"
                },
                Data = new Dictionary<string, string>()
                {
                    ["route"] = "SellerOrderList"
                },
                Topic = "USER_" + seller.Id
            };
            await messaging.SendAsync(notiMessage);
            return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Tạo yêu cầu hủy đơn hàng thành công", Data = null });
        }

        [HttpPost("/api/seller/order/{orderId:int}")]
        public async Task<IActionResult> ConfirmOrderAsync(int orderId, [FromBody] ConfirmOrderRequest request)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "Bạn không có quyền truy cập nội dung này" });
            }

            if (user.Role != (int)Role.Seller)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "Bạn không phải là người bán, không thể truy cập nội dung này" });
            }

            if (request.Status == (int)OrderStatus.CancelledBySeller)
            {
                _orderService.SellerCancelOrder(id, orderId, request.message);

                var order = _orderService.Get(orderId);
                var buyer = _userService.Get((int)order.BuyerId);

                // Get HTML template
                string fullPath = Path.Combine(_templatesPath, "SellerCancelEmail.html");
                StreamReader str = new StreamReader(fullPath);
                string mailText = str.ReadToEnd();
                str.Close();
                mailText = mailText.Replace("[orderId]", orderId.ToString())
                    .Replace("[shippingAddress]", order.RecipientAddress)
                    .Replace("[totalAmount]", order.TotalAmount.ToString())
                    .Replace("[shippingCost]", order.ShippingCost.ToString())
                    .Replace("[sumAmount]", (order.TotalAmount + order.ShippingCost).ToString());

                var message = new Service.EmailService.Message(new string[] { user.Email, buyer.Email }, $"Đơn hàng #{orderId} đã bị hủy", mailText);
                await _emailService.SendEmail(message);
                var notiMessage = new FirebaseAdmin.Messaging.Message()
                {
                    Notification = new Notification
                    {
                        Title = "Đơn hàng #" + orderId,
                        Body = "Đã bị người bán hủy!"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        ["route"] = "BuyerOrderList"
                    },
                    Topic = "USER_" + buyer.Id
                };
                await messaging.SendAsync(notiMessage);
                return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Hủy đơn hàng thành công", Data = null });
            }

            if (request.Status == (int)OrderStatus.ReadyForPickup)
            {
                await _orderService.CreateShippingOrder(orderId);

                var order = _orderService.Get(orderId);
                var buyer = _userService.Get((int)order.BuyerId);

                // Get HTML template
                string fullPath = Path.Combine(_templatesPath, "OrderConfirmEmail.html");
                StreamReader str = new StreamReader(fullPath);
                string mailText = str.ReadToEnd();
                str.Close();
                mailText = mailText.Replace("[orderId]", orderId.ToString())
                    .Replace("[shippingAddress]", order.RecipientAddress)
                    .Replace("[totalAmount]", order.TotalAmount.ToString())
                    .Replace("[shippingCost]", order.ShippingCost.ToString())
                    .Replace("[sumAmount]", (order.TotalAmount + order.ShippingCost).ToString());

                var message = new Service.EmailService.Message(new string[] { user.Email, buyer.Email }, $"Đơn hàng #{orderId} đã được xác nhận", mailText);
                await _emailService.SendEmail(message);
                var notiMessage = new FirebaseAdmin.Messaging.Message()
                {
                    Notification = new Notification
                    {
                        Title = "Đơn hàng #" + orderId,
                        Body = "Đã được xác nhận!"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        ["route"] = "BuyerOrderList"
                    },
                    Topic = "USER_" + buyer.Id
                };
                await messaging.SendAsync(notiMessage);
                return Ok(new BaseResponse { Code = (int)HttpStatusCode.OK, Message = "Xác nhận đơn hàng thành công", Data = null });
            }
            return BadRequest(new ErrorDetails { StatusCode = (int)HttpStatusCode.BadRequest, Message = "Trạng thái không khớp với thông tin yêu cầu" });
        }

        //[HttpPost("/api/seller/order/deliver/{orderId:int}")]
        //public async Task<IActionResult> DefaultShippingOrderAsync(int orderId) {
        //    var id = GetUserIdFromToken();
        //    var user = _userService.Get(id);

        //    if (user == null) {
        //        return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not allowed to access this" });
        //    }

        //    if (user.Role != (int) Role.Seller) {
        //        return Unauthorized(new ErrorDetails { StatusCode = (int) HttpStatusCode.Unauthorized, Message = "You are not seller, not allowed to access this" });
        //    }
        //    await _orderService.DefaultShippingOrder(orderId);
        //    return Ok(new BaseResponse { 
        //        Code = (int) HttpStatusCode.OK, 
        //        Message = "Default order shipping applied", 
        //        Data = null 
        //    });
        //}

        [HttpPost("/api/buyer/order/done/{orderId:int}")]
        public async Task<IActionResult> BuyerConfirmDoneOrder(int orderId)
        {
            var id = GetUserIdFromToken();
            var user = _userService.Get(id);

            if (user == null)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "Bạn không có quyền truy cập nội dung này" });
            }

            if (user.Role != (int)Role.Seller && user.Role != (int)Role.Buyer)
            {
                return Unauthorized(new ErrorDetails { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "Bạn không phải là người mua, không thể truy cập nội dung này" });
            }

            var order = _orderService.Get(orderId);
            if (order == null)
            {
                return NotFound(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Không tìm thấy đơn hàng"
                });
            }
            if (order.BuyerId != id)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không được phép hoàn thành đơn hàng này"
                });
            }
            if (order.Status != (int)OrderStatus.Delivered)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không được phép hoàn thành đơn hàng này"
                });
            }
            _orderService.DoneOrder(orderId);

            var seller = _userService.Get((int)order.SellerId);

            // Get HTML template
            string fullPath = Path.Combine(_templatesPath, "DoneOrderEmail.html");
            StreamReader str = new StreamReader(fullPath);
            string mailText = str.ReadToEnd();
            str.Close();
            mailText = mailText.Replace("[orderId]", orderId.ToString())
                .Replace("[shippingAddress]", order.RecipientAddress)
                .Replace("[totalAmount]", order.TotalAmount.ToString())
                .Replace("[shippingCost]", order.ShippingCost.ToString())
                .Replace("[sumAmount]", (order.TotalAmount + order.ShippingCost).ToString());

            var message = new Service.EmailService.Message(new string[] { user.Email, seller.Email }, $"Đơn hàng #{orderId} đã hoàn thành", mailText);
            await _emailService.SendEmail(message);
            var notiMessage = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = "Đơn hàng #" + orderId,
                    Body = "Đã hoàn thành!"
                },
                Data = new Dictionary<string, string>()
                {
                    ["route"] = "SellerOrderList"
                },
                Topic = "USER_" + seller.Id
            };
            await messaging.SendAsync(notiMessage);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Hoàn tất đơn hàng thành công",
                Data = null
            });
        }
    }
}
