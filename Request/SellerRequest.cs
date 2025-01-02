using System.ComponentModel.DataAnnotations;

namespace Request
{
    public class SellerRequest
    {
        [Required(ErrorMessage = "Không được để trống họ tên")]
        public string Name { get; set; } = null!;
        [Required(ErrorMessage = "Không được để trống số điện thoại")]
        [RegularExpression(@"^(\+84|84|0[1-9]|84[1-9]|\+84[1-9])+([0-9]{8})\b$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        [Required(ErrorMessage = "Không được để trống tên cửa hàng")]
        public string RecipientName { get; set; }
        [Required(ErrorMessage = "Không được để trống số điện thoại liên lạc")]
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
    }
}
