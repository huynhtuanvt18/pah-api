using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public interface IAuctionDAO
    {
        public IQueryable<Auction> GetAuctions();
        public Auction GetAuctionById(int id);
        public IQueryable<Auction> GetAuctionAssigned(int staffId);
        public IQueryable<Auction> GetAuctionsByProductId(int productId);
        public IQueryable<Auction> GetAuctionJoined(int bidderId);
        public IQueryable<Auction> GetAuctionBySellerId(int sellerId);
        public void CreateAuction(Auction auction);
        public void UpdateAuction(Auction auction);
    }
}
