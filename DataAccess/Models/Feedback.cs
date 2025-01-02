using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Feedback
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? BuyerId { get; set; }
        public double? Ratings { get; set; }
        public string? BuyerFeedback { get; set; }
        public int Status { get; set; }
        public DateTime? Timestamp { get; set; }

        public virtual Buyer? Buyer { get; set; }
        public virtual Product? Product { get; set; }
        public virtual Response? Response { get; set; }
    }
}
