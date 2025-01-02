using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request {
    public class ForgotPasswordRequest {
        [Required(ErrorMessage = "Không được để trống email")]
        [EmailAddress(ErrorMessage = "Email định dạng không hợp lệ")]
        public string Email { get; set; }
    }

    public class ResetPasswordRequest {
        [Required(ErrorMessage = "Không được để trống mật khẩu")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Mật khẩu cần có ít nhất 8 ký tự, ít nhất 1 số, 1 chữ cái thường và 1 chữ cái in hoa")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận chưa trùng khớp với mật khẩu mới")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Không được để trống email")]
        [EmailAddress(ErrorMessage = "Email định dạng không hợp lệ")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Không được để trống mã xác nhận")]
        public string Token { get; set; }
    }
}
