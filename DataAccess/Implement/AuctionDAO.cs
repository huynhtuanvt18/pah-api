using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement
{
    public class AuctionDAO : DataAccessBase<Auction>, IAuctionDAO
    {
        public AuctionDAO(PlatformAntiquesHandicraftsContext context) : base(context) { }

        public IQueryable<Auction> GetAuctions()
        {
            return GetAll()
                .Include(a => a.Product)
                .Include(a => a.Staff)
                .Include(a => a.Product.Category)
                .Include(a => a.Product.Material)
                .Include(a => a.Product.Seller);
        }

        public IQueryable<Auction> GetAuctionAssigned(int staffId)
        {
            return GetAll()
                .Include(a => a.Product)
                .Include(a => a.Staff)
                .Include(a => a.Product.Category)
                .Include(a => a.Product.Material)
                .Include(a => a.Product.Seller)
                .Where(a => a.StaffId == staffId);
        }

        public Auction GetAuctionById(int id)
        {
            return GetAll()
                .Include(a => a.Product)
                .Include(a => a.Staff)
                .Include(a => a.Product.Category)
                .Include(a => a.Product.Material)
                .Include(a => a.Product.Seller)
                .FirstOrDefault(a => a.Id == id);
        }

        public IQueryable<Auction> GetAuctionsByProductId(int productId)
        {
            return GetAll()
                .Include(a => a.Product)
                .Include(a => a.Staff)
                .Include(a => a.Product.Category)
                .Include(a => a.Product.Material)
                .Include(a => a.Product.Seller)
                .Where(a => a.ProductId == productId);
        }

        public IQueryable<Auction> GetAuctionBySellerId(int sellerId)
        {
            return GetAll()
                .Include(a => a.Product)
                .Include(a => a.Staff)
                .Include(a => a.Product.Category)
                .Include(a => a.Product.Material)
                .Include(a => a.Product.Seller)
                .Where(a => a.Product.SellerId == sellerId);
        }

        public IQueryable<Auction> GetAuctionJoined(int bidderId)
        {
            return GetAll()
                .Include(a => a.Product)
                .Include(a => a.Staff)
                .Include(a => a.Product.Category)
                .Include(a => a.Product.Material)
                .Include(a => a.Product.Seller)
                .Where(a => a.Bids.Any(b => b.BidderId == bidderId));
                //.Where(a => a.Bids.Any(b => b.BidderId == bidderId && b.Status == (int)BidStatus.Active) && a.Bids.Any(b => b.BidderId == bidderId));
        }

        public void CreateAuction(Auction auction)
        {
            Create(auction);
        }

        public void UpdateAuction(Auction auction)
        {
            Update(auction);
        }

        public void ChangeStatusAuction(Auction auction)
        {
            Update(auction);
        }
    }
}
