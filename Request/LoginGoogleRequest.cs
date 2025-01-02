using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request
{
    public class LoginGoogleRequest
    {
        [Required(ErrorMessage = "Không được để trống email")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Không được để trống họ tên người dùng")]
        public string Name { get; set; } = null!;
        public string? ProfilePicture { get; set; }
    }
}
