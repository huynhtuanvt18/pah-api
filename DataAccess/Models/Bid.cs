using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Bid
    {
        public int Id { get; set; }
        public int? AuctionId { get; set; }
        public int? BidderId { get; set; }
        public decimal? BidAmount { get; set; }
        public DateTime? BidDate { get; set; }
        public int Status { get; set; }

        public virtual Auction? Auction { get; set; }
        public virtual Buyer? Bidder { get; set; }
    }
}
