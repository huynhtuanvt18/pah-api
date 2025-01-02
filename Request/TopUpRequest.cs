using System.ComponentModel.DataAnnotations;

namespace Request
{
    public class TopUpRequest
    {
        [Required(ErrorMessage = "Không được để trống số tiền muốn nạp")]
        [Range(20000, Double.MaxValue, ErrorMessage = "Số tiền nạp ít nhất là 20.000VNĐ")]
        public decimal? AvailableBalance { get; set; }
    }
}
