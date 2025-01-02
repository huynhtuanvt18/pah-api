using Respon.UserRes;

namespace Respon.AuctionRes
{
    public class AuctionDetailResponse
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public string StaffName { get; set; }
        public string Title { get; set; } = null!;
        public decimal EntryFee { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal? CurrentPrice { get; set; }
        public decimal Step { get; set; }
        public int Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? RegistrationStart { get; set; }
        public DateTime? RegistrationEnd { get; set; }
        public int NumberOfBids { get; set; }
        public int NumberOfBidders { get; set; }
        public List<string> ImageUrls { get; set; }
        public WinnerResponse Winner { get; set; }

        public ProductRes.ProductResponse Product { get; set; }
        public SellerRes.SellerWithAddressResponse Seller { get; set; }
    }
}
