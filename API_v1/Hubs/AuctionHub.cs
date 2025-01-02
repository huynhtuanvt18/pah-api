using API.ErrorHandling;
using DataAccess;
using DataAccess.Models;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Service;
using System.Net;

namespace API.Hubs
{

    public class AuctionHub : Hub
    {
        private readonly IAuctionService _auctionService;
        private readonly IUserService _userService;
        public AuctionHub(IAuctionService auctionService, IUserService userService)
        {
            _auctionService = auctionService;
            _userService = userService;
        }

        [Authorize]
        public async override Task OnConnectedAsync()
        {
            var userId = int.Parse(Context.User?.Claims?.FirstOrDefault(p => p.Type == "UserId")?.Value);
            var user = _userService.Get(userId);

            // Add buyer to joined auction room
            if (user.Role == (int)Role.Buyer || user.Role == (int)Role.Seller)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "USER_" + userId);
                await Clients.Group("USER_" + userId).SendAsync("ReceiveMessage", userId, "You have been added to group " + "USER_" + userId);
                await Clients.Group("USER_" + userId).SendAsync("ReceiveSubscribe",  "USER_" + userId);

                List<Auction> auctionList = _auctionService.GetAuctionJoined(userId);
                
                foreach (var auction in auctionList)
                {
                    if (auction.Status == (int)AuctionStatus.RegistrationOpen || auction.Status == (int)AuctionStatus.Opened)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "AUCTION_" + auction.Id);
                        await Clients.Group("AUCTION_" + auction.Id).SendAsync("ReceiveMessage", userId, "You have been added to group " + "AUCTION_" + auction.Id);
                        await Clients.Group("AUCTION_" + auction.Id).SendAsync("ReceiveSubscribe", "AUCTION_" + auction.Id);
                    }
                }
            }

            // Add seller to own auction room
            if (user.Role == (int)Role.Seller)
            {
                List<Auction> auctionList = _auctionService.GetAuctionBySellerId(userId, -1);
                
                foreach (var auction in auctionList)
                {
                    if (auction.Status == (int)AuctionStatus.RegistrationOpen || auction.Status == (int)AuctionStatus.Opened)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "AUCTION_" + auction.Id);
                        await Clients.Group("AUCTION_" + auction.Id).SendAsync("ReceiveMessage", userId, "You have been added to group " + "AUCTION_" + auction.Id);
                        await Clients.Group("AUCTION_" + auction.Id).SendAsync("ReceiveSubscribe", "AUCTION_" + auction.Id);
                    }
                }
            }

            // Add staff to manage auction room
            if (user.Role == (int)Role.Staff)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "STAFF_" + userId);
                await Clients.Group("STAFF_" + userId).SendAsync("ReceiveMessage", userId, "You have been added to group " + "STAFF_" + userId);
                List<Auction> auctionList = _auctionService.GetAuctionAssigned(userId);

                foreach (var auction in auctionList)
                {
                    if (auction.Status == (int)AuctionStatus.RegistrationOpen || auction.Status == (int)AuctionStatus.Opened
                        || auction.Status == (int)AuctionStatus.Assigned)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "AUCTION_" + auction.Id);
                        await Clients.Group("AUCTION_" + auction.Id).SendAsync("ReceiveMessage", userId, "You have been added to group " + "AUCTION_" + auction.Id);
                    }
                }
            }


            await base.OnConnectedAsync();
        }

        [Authorize]
        public async Task SendMessage(string user, string message)
        {
            var userId = int.Parse(Context.User?.Claims?.FirstOrDefault(p => p.Type == "UserId")?.Value);
            await Clients.All.SendAsync("ReceiveMessage", userId, message);
        }

        public async Task JoinGroup(string group)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("ReceiveMessage", "SYSTEM", "You have been added to group " + group);
        }

        [Authorize]
        public async Task PlaceBidSuccess(int auctionId)
        {
            var userId = int.Parse(Context.User?.Claims?.FirstOrDefault(p => p.Type == "UserId")?.Value);
            var user = _userService.Get(userId);
            var auction = _auctionService.GetAuctionById(auctionId);

            await Clients.Group("AUCTION_" + auctionId).SendAsync("ReceiveNewBid", user.Name, auction.Title);
        }
    }

}