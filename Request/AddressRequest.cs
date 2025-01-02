using System.ComponentModel.DataAnnotations;

namespace Request {
    public class AddressRequest {
        public int Id { get; set; }
        [Required(ErrorMessage = "Không được để trống tên người nhận hàng")]
        public string RecipientName { get; set; }
        [Required(ErrorMessage = "Không được để trống số điện thoại người nhận hàng")]
        [RegularExpression(@"^(\+84|84|0[1-9]|84[1-9]|\+84[1-9])+([0-9]{8})\b$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string RecipientPhone { get; set; }
        [Required(ErrorMessage = "Không được để trống tỉnh thành")]
        public string Province { get; set; }
        [Required(ErrorMessage = "Không được để trống tỉnh thành")]
        public int ProvinceId { get; set; }
        [Required(ErrorMessage = "Không được để trống quận huyện")]
        public string District { get; set; }
        [Required(ErrorMessage = "Không được để trống quận huyện")]
        public int DistrictId { get; set; }
        [Required(ErrorMessage = "Không được để trống phường xã")]
        public string Ward { get; set; }
        [Required(ErrorMessage = "Không được để trống phường xã")]
        public string WardCode { get; set; }
        [Required(ErrorMessage = "Không được để trống tên đường")]
        public string Street { get; set; }
        [Required(ErrorMessage = "Không được để trống loại địa chỉ")]
        [Range(1 , 2, ErrorMessage = "Địa chỉ chỉ có thể là giao hàng hoặc lấy hàng")]
        public int Type { get; set; }
        public bool? IsDefault { get; set; }
    }
}
