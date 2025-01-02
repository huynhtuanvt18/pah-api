using Request.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Không được để trống mật khẩu cũ")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Không được để trống mật khẩu mới")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Mật khẩu cần có ít nhất 8 ký tự, ít nhất 1 số, 1 chữ cái thường và 1 chữ cái in hoa")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Không được để trống mật khẩu xác nhận")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và mật khẩu nhập lại không trùng khớp")]
        public string ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OldPassword == NewPassword)
            {
                yield return new ValidationResult("Mật khẩu mới không được trùng mật khẩu cũ",
                    new[] { nameof(OldPassword), nameof(NewPassword) });
            }
        }
    }
}
