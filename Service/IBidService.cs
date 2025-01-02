using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IBidService
    {
        public List<Bid> GetAllBidsFromAuction(int auctionId, int status);
        public int GetNumberOfBids(int auctionId);
        public int GetNumberOfBidders(int auctionId);
        public int GetNumberOfParticipants(int auctionId);
        public Bid GetHighestBidFromAuction(int auctionId);
        public bool PlaceBid(int auctionId, Bid bid);
        public void RegisterToJoinAuction(int bidderId, int auctionId);
        public void RetractBid(int auctionId, int bidderId);
    }
}
