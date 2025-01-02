using DataAccess.Models;
using Respon.BidderRes;
using Respon.UserRes;

namespace Respon.BidRes
{
    public class BidResponse
    {
        public int Id { get; set; }
        public int? AuctionId { get; set; }
        public int? BidderId { get; set; }
        public decimal? BidAmount { get; set; }
        public DateTime? BidDate { get; set; }
        public int Status { get; set; }
        public BidderResponse Bidder {  get; set; }
    }
}
