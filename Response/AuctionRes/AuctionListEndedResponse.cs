using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respon.AuctionRes
{
    public class AuctionListEndedResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Title { get; set; } = null!;
        public decimal EntryFee { get; set; }
        public int Status { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal? CurrentPrice { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? RegistrationStart { get; set; }
        public DateTime? RegistrationEnd { get; set; }
        public int NumberOfBidders { get; set; }
        public string ImageUrl { get; set; }
    }
}
