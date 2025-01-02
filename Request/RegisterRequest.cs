using System.ComponentModel.DataAnnotations;

namespace Request {
    public class RegisterRequest {
        [Required(ErrorMessage = "Không được để trống họ tên người dùng")]
        public string Name { get; set; } = null!;
        [Required(ErrorMessage = "Không được để trống email")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Không được để trống mật khẩu")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Mật khẩu cần có ít nhất 8 ký tự, ít nhất 1 số, 1 chữ cái thường và 1 chữ cái in hoa")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "Không được để trống số điện thoại")]
        [RegularExpression(@"^(\+84|84|0[1-9]|84[1-9]|\+84[1-9])+([0-9]{8})\b$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }
        public int? Gender { get; set; }
        public DateTime? Dob { get; set; }
    }

    public class VerificationRequest {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Code { get; set; }
    }
}
