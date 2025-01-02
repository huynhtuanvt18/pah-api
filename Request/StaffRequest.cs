using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request {
    public class StaffRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Không được để trống họ tên người dùng")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Không được để trống email")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Không được để trống mật khẩu")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Mật khẩu cần có ít nhất 8 ký tự, ít nhất 1 số, 1 chữ cái thường và 1 chữ cái in hoa")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Không được để trống số điện thoại")]
        [RegularExpression(@"^(\+84|84|0[1-9]|84[1-9]|\+84[1-9])+([0-9]{8})\b$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }
        public string? ProfilePicture { get; set; }
        [Required(ErrorMessage = "Không được để trống giới tính")]
        public int? Gender { get; set; }
        [Required(ErrorMessage = "Không được để trống ngày tháng năm sinh")]
        public DateTime Dob { get; set; }
        [Required(ErrorMessage = "Không được để trống trạng thái")]
        public int Status { get; set; }
        [Required(ErrorMessage = "Không được để trống vai trò")]
        [Range(4,5, ErrorMessage = "Vai trò phải là Nhân viên hoặc Quản lý")]
        public int Role { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            int age = DateTime.Now.Year - Dob.Year;
            if (DateTime.Now.DayOfYear < Dob.DayOfYear) age = age - 1;
            if (age < 18)
            {
                yield return new ValidationResult($"Người dùng phải hơn 18 tuổi", new[] { nameof(Dob) });
            }
        }
    }
}
