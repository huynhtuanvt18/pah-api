using System.ComponentModel.DataAnnotations;

namespace API.Request
{
    public class BidRequest
    {
        [Required(ErrorMessage = "Không được để trống ID đấu giá")]
        public int? AuctionId { get; set; }
        [Required(ErrorMessage = "Không được để trống số tiền đấu giá")]
        [Range(1000, Double.MaxValue, ErrorMessage ="Số tiền đấu giá phải lớn hơn 1.000VNĐ")]
        public decimal? BidAmount { get; set; }
    }
}
