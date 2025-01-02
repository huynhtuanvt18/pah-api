using AutoMapper;
using DataAccess;
using DataAccess.Models;
using API.Request;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Request.Param;
using Respon;
using Respon.BidRes;
using Service;
using System.Net;
using API.ErrorHandling;
using Respon.UserRes;
using API.Hubs;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class BidController : ControllerBase
    {
        private readonly IBidService _bidService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IAuctionService _auctionService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private IHubContext<AuctionHub> _hubContext { get; set; }
        private readonly FirebaseMessaging messaging;

        public BidController(IBidService bidService, IMapper mapper, IUserService userService,
            IAuctionService auctionService, IBackgroundJobClient backgroundJobClient,
            IHubContext<AuctionHub> hubcontext)
        {
            _bidService = bidService;
            _mapper = mapper;
            _userService = userService;
            _auctionService = auctionService;
            _backgroundJobClient = backgroundJobClient;
            _hubContext = hubcontext;
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

        [HttpGet("auction/{id}")]
        public IActionResult GetBidsFromAuction(int id, [FromQuery] int status, [FromQuery] PagingParam pagingParam) 
        {
            List<Bid> bidList = _bidService.GetAllBidsFromAuction(id, status)
                .Skip((pagingParam.PageNumber - 1) * pagingParam.PageSize).Take(pagingParam.PageSize).ToList();

            List<BidResponse> response = _mapper.Map<List<BidResponse>>(bidList);
            foreach (var bid in response)
            {
                var bidder = _userService.Get((int)bid.BidderId);
                if (bidder == null)
                {
                    bid.Bidder = null;
                }
                else
                {
                    bid.Bidder.Name = bidder.Name;
                    bid.Bidder.Email = bidder.Email;
                    bid.Bidder.Phone = bidder.Phone;
                    bid.Bidder.ProfilePicture = bidder.ProfilePicture;
                    bid.Bidder.Gender = bidder.Gender;
                    bid.Bidder.Dob = bidder.Dob;
                    bid.Bidder.Role = bidder.Role;
                    bid.Bidder.Status = bidder.Status;
                    //bid.Bidder = _mapper.Map<UserResponse>(bidder);
                }
            }
            return Ok(new BaseResponse 
            { 
                Code = (int)HttpStatusCode.OK, 
                Message = "Lấy danh sách những lần đặt giá thành công",
                Data = response
            });
        }

        [Authorize]
        [HttpPost]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult PlaceBid([FromBody] BidRequest request)
        {
            var bidderId = GetUserIdFromToken();
            var bidder = _userService.Get(bidderId);
            if (bidder == null || (bidder.Role != (int)Role.Buyer && bidder.Role != (int)Role.Seller))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }

            bool endTimeChanged = _bidService.PlaceBid(bidderId, _mapper.Map<Bid>(request));

            if (endTimeChanged)
            {
                var auction = _auctionService.GetAuctionById((int)request.AuctionId);
                DateTime endTime = (DateTime)auction.EndedAt;

                // Set new schedule for auction end
                _backgroundJobClient.Schedule(() => EndAuction(auction.Id, true), endTime.AddSeconds(-5));
                _backgroundJobClient.Schedule(() => NotifyEndAuction(auction.Id), endTime.AddSeconds(-35));
            }
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Đặt giá thành công",
                Data = null
            });
        }

        [Authorize]
        [HttpPost("auction/register/{id}")]
        public async Task<IActionResult> RegisterAuction(int id)
        {
            var bidderId = GetUserIdFromToken();
            var bidder = _userService.Get(bidderId);
            if (bidder == null || (bidder.Role != (int)Role.Buyer && bidder.Role != (int)Role.Seller))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            _bidService.RegisterToJoinAuction(bidderId, id);
            var notiMessage = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = "Đăng ký tham gia đấu giá thành công",
                    Body = "Phí tham gia đấu giá đã bị trừ vào tài khoản của bạn!"
                },
                Topic = "USER_" + bidderId
            };
            await messaging.SendAsync(notiMessage);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Đăng kí tham gia cuộc đấu giá thành công",
                Data = null
            });
        }

        [Authorize]
        [HttpGet("retract/{id}")]
        public IActionResult RetractBid(int id)
        {
            var bidderId = GetUserIdFromToken();
            var bidder = _userService.Get(bidderId);
            if (bidder == null || (bidder.Role != (int)Role.Buyer && bidder.Role != (int)Role.Seller))
            {
                return Unauthorized(new ErrorDetails
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Bạn không có quyền truy cập nội dung này"
                });
            }
            _bidService.RetractBid(id, bidderId);
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Rút lui khỏi cuộc đấu giá thành công",
                Data = null
            });
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
            DataAccess.Models.User winner;
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
        }
    }
}
