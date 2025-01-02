using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement
{
    public class BidDAO : DataAccessBase<Bid>, IBidDAO
    {
        public BidDAO(PlatformAntiquesHandicraftsContext context) : base(context) { }

        public IQueryable<Bid> GetBidsByAuctionId(int auctionId)
        {
            return GetAll()
                .Include(b => b.Auction)
                .Include(b => b.Bidder.IdNavigation)
                .Where(b => b.AuctionId == auctionId);
        }

        public void CreateBid(Bid bid)
        {
            Create(bid);
        }
        public void UpdateBid(Bid bid)
        {
            Update(bid);
        }
    }
}
