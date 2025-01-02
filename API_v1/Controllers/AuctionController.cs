using API.ErrorHandling;
using API.Hubs;
using AutoMapper;
using DataAccess;
using DataAccess.Implement;
using DataAccess.Models;
using Hangfire;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Request;
using Request.Param;
using Respon;
using Respon.AuctionRes;
using Respon.SellerRes;
using Respon.UserRes;
using Service;
using Service.EmailService;
using Service.Implement;
using System.Net;
using System.Reflection;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IBidService _bidService;
        private readonly ISellerService _sellerService;
        private readonly IAddressService _addressService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private IHubContext<AuctionHub> _hubContext { get; set; }
        private readonly IEmailService _emailService;
        private readonly string _templatesPath;
        private readonly IOrderService _orderService;
        private readonly FirebaseMessaging messaging;

        public AuctionController(IAuctionService auctionService, IMapper mapper, IUserService userService, IImageService imageService,
            IBidService bidService, ISellerService sellerService, IAddressService addressService, IBackgroundJobClient backgroundJobClient,
            IHubContext<AuctionHub> hubcontext, IEmailService emailService, IConfiguration pathConfig, IOrderService orderService)
        {
            _auctionService = auctionService;
            _mapper = mapper;
            _userService = userService;
            _imageService = imageService;
            _bidService = bidService;
            _sellerService = sellerService;
            _addressService = addressService;
            _backgroundJobClient = backgroundJobClient;
            _hubContext = hubcontext;
            _emailService = emailService;
            _templatesPath = pathConfig["Path:Templates"];
            _orderService = orderService;
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

        private SellerWithAddressResponse GetSellerResponse(int sellerId)
        {
            Seller seller = _sellerService.GetSeller(sellerId);
            SellerWithAddressResponse sellerResponse = new SellerWithAddressResponse();
            if (seller != null)
            {
                sellerResponse = _mapper.Map<SellerWithAddressResponse>(seller);
                sellerResponse.Province = null;
                sellerResponse.WardCode = null;
                sellerResponse.Ward = null;
                sellerResponse.DistrictId = null;
                sellerResponse.District = null;
                sellerResponse.Street = null;

                Address address = _addressService.GetByCustomerId(sellerId)
                    .Where(a => a.Type == (int)AddressType.Pickup && a.IsDefault == true)
                    .FirstOrDefault();

                if (address != null)
                {
                    sellerResponse.Province = address.Province;
                    sellerResponse.WardCode = address.WardCode;
                    sellerResponse.Ward = address.Ward;
                    sellerResponse.DistrictId = address.DistrictId;
                    sellerResponse.District = address.District;
                    sellerResponse.Street = address.Street;
                }
            }
            return sellerResponse;
        }

        private int CountOpenAuctions(string? title, int status, int categoryId, int materialId)
        {
            int count = 0;
            count = _auctionService.GetAuctions(title, status, categoryId, materialId, 0).Count();
            return count;
        }

        private int CountAuctions(string? title, int status, int categoryId, int materialId)
        {
            int count = 0;
            count = _auctionService.GetAllAuctions(title, status, categoryId, materialId, 0).Count();
            return count;
        }

        private int CountAssignedAuctions(int id)
        {
            int count = 0;
            count = _auctionService.GetAuctionAssigned(id).Count();
            return count;
        }

        [HttpGet]
        public IActionResult GetAuctions([FromQuery] string? title,
            //[FromQuery] int status,
            [FromQuery] int categoryId,
            [FromQuery] int materialId,
            [FromQuery] int orderBy,
            [FromQuery] PagingParam pagingParam)
        {
            List<Auction> auctionList = _auctionService.GetAuctions(title, (int)AuctionStatus.RegistrationOpen, categoryId, materialId, orderBy)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<AuctionListResponse> mappedList = _mapper.Map<List<AuctionListResponse>>(auctionList);

            foreach (var item in mappedList)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
            }

            int count = CountOpenAuctions(title, (int)AuctionStatus.RegistrationOpen, categoryId, materialId);

            AuctionListCountResponse response = new AuctionListCountResponse()
            {
                Count = count,
                AuctionList = mappedList
            };

            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách các cuộc đấu giá thành công",
                Data = response
            });
        }

        [Authorize]
        [HttpGet("manager")]
        public IActionResult ManagerGetAuctions([FromQuery] string? title,

            [FromQuery] int categoryId,
            [FromQuery] int materialId,
            [FromQuery] int orderBy,
            [FromQuery] PagingParam pagingParam, [FromQuery] int status = -1)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            List<Auction> auctionList = _auctionService.GetAllAuctions(title, status, categoryId, materialId, orderBy)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<AuctionListResponse> mappedList = _mapper.Map<List<AuctionListResponse>>(auctionList);

            foreach (var item in mappedList)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
            }

            int count = CountAuctions(title, status, categoryId, materialId);

            AuctionListCountResponse response = new AuctionListCountResponse()
            {
                Count = count,
                AuctionList = mappedList
            };

            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách các cuộc đấu giá thành công",
                Data = response
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetAuctionById(int id)
        {
            Auction auction = _auctionService.GetAuctionById(id);

            if (auction == null)
            {
                return NotFound(new ErrorDetails
                {
                    StatusCode = 400,
                    Message = "Cuộc đấu giá này không tồn tại"
                });
            }

            List<ProductImage> imageList = _imageService.GetAllImagesByProductId((int)auction.ProductId);
            List<string> imageUrls = imageList.Select(i => i.ImageUrl).ToList();
            AuctionDetailResponse response = _mapper.Map<AuctionDetailResponse>(auction);
            response.ImageUrls = imageUrls;

            response.NumberOfBids = _bidService.GetNumberOfBids(id);
            response.NumberOfBidders = _bidService.GetNumberOfBidders(id);

            Bid highestBid = _bidService.GetHighestBidFromAuction(auction.Id);
            response.CurrentPrice = response.StartingPrice;
            if (highestBid != null)
            {
                response.CurrentPrice = highestBid.BidAmount;
            }

            response.Seller = GetSellerResponse((int)auction.Product.SellerId);

            Bid winnerBid = _bidService.GetHighestBidFromAuction(id);
            if(winnerBid != null)
            {
                WinnerResponse winner = _mapper.Map<WinnerResponse>(_userService.Get((int)winnerBid.BidderId));
                winner.FinalBid = winnerBid.BidAmount;

                response.Winner = winner;
            }
            else
            {
                WinnerResponse winner = new WinnerResponse();
                response.Winner = winner;
            }
            
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách các cuộc đấu giá thành công",
                Data = response
            });
        }

        [HttpGet("seller/{id}")]
        public IActionResult GetAuctionBySellerId(int id, [FromQuery] int status, [FromQuery] PagingParam pagingParam)
        {
            List<Auction> auctionList = _auctionService.GetAuctionBySellerId(id, status)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<AuctionListResponse> response = _mapper.Map<List<AuctionListResponse>>(auctionList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
            }
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách các cuộc đấu giá thành công",
                Data = response
            });
        }

        [Authorize]
        [HttpGet("seller/current")]
        public IActionResult GetAuctionByCurrentSeller([FromQuery] int status, [FromQuery] PagingParam pagingParam)
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
            List<Auction> auctionList = _auctionService.GetAuctionBySellerId(userId, status)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<AuctionListResponse> response = _mapper.Map<List<AuctionListResponse>>(auctionList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
            }
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách các cuộc đấu giá thành công",
                Data = response
            });
        }

        [Authorize]
        [HttpGet("staff")]
        public IActionResult GetAuctionAssigned([FromQuery] PagingParam pagingParam)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Staff && user.Role != (int)Role.Administrator))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            List<Auction> auctionList = _auctionService.GetAuctionAssigned(userId)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();
            List<AuctionListResponse> mappedList = _mapper.Map<List<AuctionListResponse>>(auctionList);
            foreach (var item in mappedList)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
            }
            AuctionListCountResponse response = new AuctionListCountResponse()
            {
                Count = CountAssignedAuctions(userId),
                AuctionList = mappedList
            };
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách các cuộc đấu giá thành công",
                Data = response
            });
        }

        [HttpGet("bidder")]
        public IActionResult GetAuctionsByBidderId([FromQuery] PagingParam pagingParam, [FromQuery] int status = -1)
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
            List<Auction> auctionList = _auctionService.GetAuctionJoinedByStatus(status, userId)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();

            List<AuctionListBidderResponse> response = _mapper.Map<List<AuctionListBidderResponse>>(auctionList);
            foreach (var item in response)
            {
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
                item.IsWon = _auctionService.CheckWinner(userId, item.Id);
            }
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách các cuộc đấu giá đã tham dự thành công",
                Data = response
            });
        }

        [Authorize]
        [HttpGet("staff/ended/{month}")]
        public IActionResult GetAuctionsEndedCurrentStaff(int month)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Staff)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You are not allowed to access this"
                });
            }
            List<Auction> auctionList = _auctionService.GetAuctionsDoneAssignedByMonths(userId, month);
            List<AuctionListEndedResponse> mappedList = _mapper.Map<List<AuctionListEndedResponse>>(auctionList);
            foreach (var item in mappedList)
            {
                item.NumberOfBidders = _bidService.GetNumberOfParticipants(item.Id);
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
            }
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách các cuộc đấu giá đã kết thúc thành công",
                Data = mappedList
            });
        }

        [Authorize]
        [HttpGet("manager/ended/{month}")]
        public IActionResult GetAuctionsEndedAllStaff(int month)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || (user.Role != (int)Role.Manager && user.Role != (int)Role.Administrator))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            List<Auction> auctionList = _auctionService.GetAuctionsDoneByMonths(month);
            List<AuctionListEndedResponse> mappedList = _mapper.Map<List<AuctionListEndedResponse>>(auctionList);
            foreach (var item in mappedList)
            {
                item.NumberOfBidders = _bidService.GetNumberOfParticipants(item.Id);
                ProductImage image = _imageService.GetMainImageByProductId(item.ProductId);
                if (image == null)
                {
                    item.ImageUrl = null;
                }
                else
                {
                    item.ImageUrl = image.ImageUrl;
                }

                Bid highestBid = _bidService.GetHighestBidFromAuction(item.Id);
                item.CurrentPrice = item.StartingPrice;
                if (highestBid != null)
                {
                    item.CurrentPrice = highestBid.BidAmount;
                }
            }
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách các cuộc đấu giá đã kết thúc thành công",
                Data = mappedList
            });
        }

        [HttpPost]
        public IActionResult CreateAuction([FromBody] AuctionRequest request)
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
            _auctionService.CreateAuction(_mapper.Map<Auction>(request));
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Thêm mới cuộc đấu giá thành công",
                Data = null
            });
        }

        [Authorize]
        [HttpGet("register/check/{id}")]
        public IActionResult CheckAuctionRegistration(int id)
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
            bool check = _auctionService.CheckRegistration(userId, id);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Kiểm tra đăng kí cuộc đấu giá thành công",
                Data = check
            });
        }

        [Authorize]
        [HttpGet("win/check/current/{id}")]
        public IActionResult CheckCurrentUserWinAuction(int id)
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
            bool check = _auctionService.CheckWinner(userId, id);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Kiểm tra chiến thắng cuộc đấu giá thành công",
                Data = check
            });
        }

        [Authorize]
        [HttpGet("win/check/{id}")]
        public IActionResult CheckUserWinAuction(int id, [FromQuery] int userId)
        {
            //var userId = GetUserIdFromToken();
            //var user = _userService.Get(userId);
            //if (user == null)
            //{
            //    return Unauthorized(new ErrorDetails
            //    {
            //        StatusCode = (int)HttpStatusCode.Unauthorized,
            //        Message = "You are not allowed to access this"
            //    });
            //}
            bool check = _auctionService.CheckWinner(userId, id);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Kiểm tra chiến thắng cuộc đấu giá thành công",
                Data = check
            });
        }

        [Authorize]
        [HttpGet("end/{id}")]
        public IActionResult StaffEndAuction(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Staff)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            WinnerResponse mappedWinner = EndAuction(id, false).Result;

            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Kết thúc cuộc đấu giá thành công",
                Data = mappedWinner
            });
        }

        [Authorize]
        [HttpGet("assign")]
        public IActionResult AssignStaffToAuction(int id, int staffId)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Manager)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            _auctionService.AssignStaff(id, staffId);
            var auction = _auctionService.GetAuctionById(id);

            // Notify staff of assigned auction
            _hubContext.Clients.Group("STAFF_" + staffId).SendAsync("ReceiveAuctionAssigned", auction.Id, auction.Title);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Bàn giao cuộc đấu giá cho nhân viên thành công",
                Data = null
            });
        }

        [Authorize]
        [HttpGet("manager/approve/{id}")]
        public async Task<IActionResult> ManagerApproveAuction(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Manager)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            _auctionService.ManagerApproveAuction(id);
            var auction = _auctionService.GetAuctionById(id);
            var notiMessage = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = auction.Title,
                    Body = "Cuộc đấu giá của bạn đã được duyệt. Một nhân viên sẽ được giao cho quản lý cuộc đấu giá của bạn."
                },
                Topic = "USER_" + auction.Product.SellerId
            };
            await messaging.SendAsync(notiMessage);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Xác nhận cuộc đấu giá thành công",
                Data = null
            });
        }

        [Authorize]
        [HttpGet("manager/reject/{id}")]
        public async Task<IActionResult> ManagerRejectAuction(int id)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Manager)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            _auctionService.ManagerRejectAuction(id);
            var auction = _auctionService.GetAuctionById(id);
            var notiMessage = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = auction.Title,
                    Body = "Cuộc đấu giá của bạn đã được duyệt. Một nhân viên sẽ được giao cho quản lý cuộc đấu giá của bạn."
                },
                Topic = "USER_" + auction.Product.SellerId
            };
            await messaging.SendAsync(notiMessage);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Từ chối cuộc đấu giá thành công",
                Data = null
            });
        }

        [HttpPost("staff/info/{id}")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult StaffSetAuctionInfo(int id, [FromBody] AuctionDateRequest request)
        {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null || user.Role != (int)Role.Staff)
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            _auctionService.StaffSetAuctionInfo(id,
                (DateTime)request.RegistrationStart,
                (DateTime)request.RegistrationEnd,
                (DateTime)request.StartedAt,
                (DateTime)request.EndedAt,
                request.Step, userId);

            DateTime endTime = (DateTime)request.EndedAt;
            DateTime startTime = (DateTime)request.StartedAt;

            _backgroundJobClient.Schedule(() => HostAuction(id, (int)AuctionStatus.Opened), startTime.AddSeconds(-5));
            _backgroundJobClient.Schedule(() => EndAuction(id, true), endTime.AddSeconds(-5));
            _backgroundJobClient.Schedule(() => NotifyEndAuction(id), endTime.AddSeconds(-35));

            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Cập nhật thông tin đấu giá thành công",
                Data = null
            });
        }

        [HttpPost("order/create")]
        public async Task<IActionResult> CreateAuctionOrder([FromBody] AuctionOrderRequest request) {
            var userId = GetUserIdFromToken();
            var user = _userService.Get(userId);
            if (user == null) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            
            if (user.Role != (int) Role.Buyer && user.Role != (int) Role.Seller) {
                return Unauthorized(new ErrorDetails {
                    StatusCode = (int) HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            var orderId = _auctionService.CreateAuctionOrder(userId, request).Result;
            var order = _orderService.Get(orderId);
            var seller = _userService.Get((int)order.SellerId);

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

            var message = new Service.EmailService.Message(new string[] { user.Email, seller.Email }, $"Đơn hàng #{orderId} đã được xác nhận", mailText);
            await _emailService.SendEmail(message);
            var notiMessage = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = "Đơn hàng #" + orderId,
                    Body = "Đơn hàng đã được xác nhận!"
                },
                Data = new Dictionary<string, string>()
                {
                    ["route"] = "SellerOrderDetail"
                },
                Topic = "USER_" + seller.Id
            };
            await messaging.SendAsync(notiMessage);
            return Ok(new BaseResponse {
                Code = (int) HttpStatusCode.OK,
                Message = "Tạo đơn hàng cho cuộc đấu giá thành công",
                Data = null
            });
        }

        // Start auction automatically
        [NonAction]
        public async Task HostAuction(int auctionId, int status)
        {
            var statusUpdated = _auctionService.HostAuction(auctionId, status);

            if (status == (int)AuctionStatus.Opened && statusUpdated)
            {
                var auction = _auctionService.GetAuctionById(auctionId);
                if (auction == null) return;
                await _hubContext.Clients.Group("AUCTION_" + auctionId).SendAsync("ReceiveAuctionOpen", auction.Title);
                var message = new FirebaseAdmin.Messaging.Message()
                {
                    Notification = new Notification
                    {
                        Title = auction.Title,
                        Body = "Cuộc đấu giá đã bắt đầu, hãy tham gia nào!"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        ["route"] = "BidderAuctionHistoryListing"
                    },
                    Topic = "AUCTION_" + auctionId
                };
                await messaging.SendAsync(message);
            }
        }

        // End Auction automatically
        [NonAction]
        public async Task<WinnerResponse?> EndAuction(int auctionId, bool scheduled = true)
        {
            var auction = _auctionService.GetAuctionById(auctionId);
            DateTime endTime = (DateTime)auction.EndedAt;
            if (scheduled && DateTime.Now < endTime.AddSeconds(-5))
            {
                throw new Exception("404: EndedDate Changed");
            }

            var winnerBid = _auctionService.EndAuction(auctionId);
            User winner;
            WinnerResponse mappedWinner;
            if (winnerBid != null)
            {
                winner = _userService.Get((int)winnerBid.BidderId);
                mappedWinner = _mapper.Map<WinnerResponse>(winner);
                mappedWinner.FinalBid = winnerBid.BidAmount;
            }
            else
            {
                mappedWinner = null;
            }

            await _hubContext.Clients.Group("AUCTION_" + auctionId).SendAsync("ReceiveAuctionEnd", auction.Title);
            var message = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = auction.Title,
                    Body = "Cuộc đấu giá đã kết thúc, hãy cùng xem người thắng cuộc nào!"
                },
                Data = new Dictionary<string, string>()
                {
                    ["route"] = "BidderAuctionHistoryListing"
                },
                Topic = "AUCTION_" + auctionId
            };
            await messaging.SendAsync(message);
            return mappedWinner;
        }

        // Notify end time
        [NonAction]
        public async Task NotifyEndAuction(int auctionId)
        {
            var auction = _auctionService.GetAuctionById(auctionId);
            DateTime endTime = (DateTime)auction.EndedAt;
            if (DateTime.Now < endTime.AddSeconds(-35))
            {
                throw new Exception("404: EndedDate Changed");
            }
            await _hubContext.Clients.Group("AUCTION_" + auctionId).SendAsync("ReceiveAuctionAboutToEnd", auction.Title);
            var message = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = auction.Title,
                    Body = "Cuộc đấu giá chuẩn bị kết thúc!"
                },
                Data = new Dictionary<string, string>()
                {
                    ["route"] = "BidderAuctionHistoryListing"
                },
                Topic = "AUCTION_" + auctionId
            };
            await messaging.SendAsync(message);
        }
    }
}
