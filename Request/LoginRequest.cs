using System.ComponentModel.DataAnnotations;

namespace Request {
    public class LoginRequest {
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        [Required(ErrorMessage = "Không được để trống email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Không được để trống mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
