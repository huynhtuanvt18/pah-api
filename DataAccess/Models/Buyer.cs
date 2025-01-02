using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Buyer
    {
        public Buyer()
        {
            Bids = new HashSet<Bid>();
            Feedbacks = new HashSet<Feedback>();
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public DateTime? JoinedAt { get; set; }

        public virtual User IdNavigation { get; set; } = null!;
        public virtual ICollection<Bid> Bids { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
