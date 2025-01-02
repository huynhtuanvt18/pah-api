using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Response
    {
        public int FeedbackId { get; set; }
        public int? SellerId { get; set; }
        public string? SellerMessage { get; set; }
        public DateTime? Timestamp { get; set; }

        public virtual Feedback Feedback { get; set; } = null!;
        public virtual Seller? Seller { get; set; }
    }
}
