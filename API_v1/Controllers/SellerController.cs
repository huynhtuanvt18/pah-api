using API.ErrorHandling;
using AutoMapper;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Request;
using Request.Param;
using Respon;
using Respon.OrderRes;
using Respon.SellerRes;
using Service;
using System.Net;
using static Google.Apis.Requests.BatchRequest;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellerController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISellerService _sellerService;
        private readonly IAddressService _addressService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IAuctionService _auctionService;
        private readonly IMapper _mapper;

        public SellerController(IUserService userService, ISellerService sellerService, IMapper mapper, IAddressService addressService, 
            IProductService productService, IOrderService orderService, IAuctionService auctionService)
        {
            _userService = userService;
            _sellerService = sellerService;
            _mapper = mapper;
            _addressService = addressService;
            _productService = productService;
            _orderService = orderService;
            _auctionService = auctionService;
        }

        private int GetUserIdFromToken()
        {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [Authorize]
        [HttpGet("current")]
        public IActionResult Get()
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
             //|| user.Role != (int)Role.Seller
            if (user == null)
            {
                return Unauthorized(new ErrorDetails
                { StatusCode =
                (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            var seller = _sellerService.GetSeller(userId);
            var address = _addressService.GetPickupBySellerId(userId);

            SellerDetailResponse response = _mapper.Map<SellerDetailResponse>(new Seller());

            if(seller != null)
            {
                response.Id = seller.Id;
                response.Name = seller.Name;
                response.Phone = seller.Phone;
                response.ProfilePicture = seller.ProfilePicture;
                response.RegisteredAt = seller.RegisteredAt;
                response.Ratings = seller.Ratings;
                response.Status = seller.Status;
                response.RecipientName = address.RecipientName;
                response.RecipientPhone = address.RecipientPhone;
                response.Province = address.Province;
                response.ProvinceId = address.ProvinceId;
                response.District = address.District;
                response.DistrictId = address.DistrictId;
                response.Ward = address.Ward;
                response.WardCode = address.WardCode;
                response.Street = address.Street;
            }
            
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy thông tin người bán thành công",
                Data = response 
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var seller = _sellerService.GetSeller(id);
            var address = _addressService.GetPickupBySellerId(id);

            SellerDetailResponse response = _mapper.Map<SellerDetailResponse>(new Seller());

            if (seller != null)
            {
                response.Id = seller.Id;
                response.Name = seller.Name;
                response.Phone = seller.Phone;
                response.ProfilePicture = seller.ProfilePicture;
                response.RegisteredAt = seller.RegisteredAt;
                response.Ratings = seller.Ratings;
                response.Status = seller.Status;
                response.RecipientName = address.RecipientName;
                response.RecipientPhone = address.RecipientPhone;
                response.Province = address.Province;
                response.ProvinceId = address.ProvinceId;
                response.District = address.District;
                response.DistrictId = address.DistrictId;
                response.Ward = address.Ward;
                response.WardCode = address.WardCode;
                response.Street = address.Street;
            }

            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy thông tin người bán thành công",
                Data = response
            });
        }

        [Authorize]
        [HttpPost]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<IActionResult> SellerRequest([FromBody] SellerRequest request)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            var shopId = await _sellerService.CreateShopIdAsync(request);

            Seller seller = new Seller()
            {
                Id = userId,
                Name = request.Name,
                Phone = request.Phone,
                ProfilePicture = request.ProfilePicture,
                Status = (int)SellerStatus.Pending,
                ShopId = shopId.ToString()
            };
            int existed = _sellerService.CreateSeller(userId, seller);

            Address address = new Address()
            {
                CustomerId = userId,
                RecipientName = request.RecipientName,
                RecipientPhone = request.RecipientPhone,
                Province = request.Province,
                ProvinceId = request.ProvinceId,
                District = request.District,
                DistrictId = request.DistrictId,
                Ward = request.Ward,
                WardCode = request.WardCode,
                Street = request.Street,
                Type = (int)AddressType.Pickup,
                IsDefault = true
            };
            if(existed == 0)
            {
                _addressService.Create(address);
                return Ok(new BaseResponse
                {
                    Code = (int)HttpStatusCode.OK,
                    Message = "Gửi yêu cầu trở thành người bán thành công",
                    Data = null
                });
            }
            else
            {
                _sellerService.UpdateSeller(seller);
                _addressService.UpdateSellerAddress(address, userId);
                return Ok(new BaseResponse
                {
                    Code = (int)HttpStatusCode.OK,
                    Message = "Cập nhật thông tin người bán thành công",
                    Data = null
                });
            }
        }

        [Authorize]
        [HttpGet("request")]
        public IActionResult GetSellerRequestList([FromQuery] PagingParam pagingParam)
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
            }
            List<Seller> sellerRequests = _sellerService.GetSellerRequestList().Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize)
                .Take(pagingParam.PageSize).ToList();
            int sellerRequestsCount = _sellerService.GetSellerRequestList().Count;
            List<SellerRequestResponse> responses = _mapper.Map<List<SellerRequestResponse>>(sellerRequests);
            foreach (var item in responses)
            {
                var pickupAddress = _addressService.GetPickupBySellerId(item.Id);
                item.Province = pickupAddress.Province;
                item.DistrictId = pickupAddress.DistrictId;
                item.District = pickupAddress.District;
                item.WardCode = pickupAddress.WardCode;
                item.Ward = pickupAddress.Ward;
                item.Street = pickupAddress.Street;

                var sellerUser = _userService.Get(item.Id);
                item.UserName = sellerUser.Name;
                item.Email = sellerUser.Email;
                item.Phone = sellerUser.Phone;
                item.Gender = sellerUser.Gender;
                item.Dob = sellerUser.Dob;
            }
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách yêu cầu trở thành người bán thành công",
                Data = new {
                    Count = sellerRequestsCount,
                    List = responses
                }
            });
        }

        [Authorize]
        [HttpGet("dashboard")]
        public IActionResult GetDashboardFromCurrentSeller()
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Seller))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            var sales = _sellerService.GetSalesCurrentSeller(userId);
            var sellingProducts = _productService.GetProductsBySellerId(userId)
                .Where(p => p.Status == (int)Status.Available && p.Type == (int)ProductType.ForSale).Count();
            var doneOrders = _orderService.GetBySellerId(userId, new List<int>() { (int)OrderStatus.Done }).Count();
            var processingOrders = _orderService.GetProcessingBySellerId(userId).Count();
            var totalOrders = _orderService.GetBySellerId(userId, new List<int>()).Count();
            var totalAuctions = _auctionService.GetAuctionBySellerId(userId, -1).Count();
            SellerSalesResponse response = new SellerSalesResponse()
            {
                TotalSales = sales,
                SellingProduct = sellingProducts,
                ProcessingOrders = processingOrders,
                DoneOrders = doneOrders,
                TotalOrders = totalOrders,
                TotalAuctions = totalAuctions,
            };

            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy doanh thu của người bán hiện tại thành công",
                Data = response
            });
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetSellersBySales([FromQuery] PagingParam pagingParam)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Staff
                && user.Role != (int)Role.Manager
                && user.Role != (int)Role.Administrator))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            List<Seller> sellerList = _sellerService.GetSellers().Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize)
                .Take(pagingParam.PageSize).ToList(); 
            List<SellerWithSalesResponse> mappedList = _mapper.Map<List<SellerWithSalesResponse>>(sellerList);
            foreach(SellerWithSalesResponse seller in mappedList)
            {
                var sales = _sellerService.GetSalesCurrentSellerAllTime(seller.Id);
                seller.Sales = sales;
            }
            List<SellerWithSalesResponse> responses = mappedList.OrderByDescending(s => s.Sales).ToList();
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách người bán theo doanh thu thành công",
                Data = responses
            });
        }
    }
}
