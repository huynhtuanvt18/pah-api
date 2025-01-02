using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respon.SellerRes
{
    public class SellerRequestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? ProfilePicture { get; set; }
        public int? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public int Status { get; set; }
        public decimal? Ratings { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public int? DistrictId { get; set; }
        public string? Ward { get; set; }
        public string? WardCode { get; set; }
        public string? Street { get; set; }
    }
}
