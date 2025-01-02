using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public interface IBidDAO
    {
        public IQueryable<Bid> GetBidsByAuctionId(int auctionId);
        public void CreateBid(Bid bid);
        public void UpdateBid(Bid bid);
    }
}
