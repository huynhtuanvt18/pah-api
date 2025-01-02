using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Auction
    {
        public Auction()
        {
            Bids = new HashSet<Bid>();
        }

        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? StaffId { get; set; }
        public string Title { get; set; } = null!;
        public decimal EntryFee { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal Step { get; set; }
        public int Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? MaxEndedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? RegistrationStart { get; set; }
        public DateTime? RegistrationEnd { get; set; }

        public virtual Product? Product { get; set; }
        public virtual User? Staff { get; set; }
        public virtual ICollection<Bid> Bids { get; set; }
    }
}
