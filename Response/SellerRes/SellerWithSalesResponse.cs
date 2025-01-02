using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respon.SellerRes
{
    public class SellerWithSalesResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public int Status { get; set; }
        public string ShopId { get; set; } = null!;
        public decimal Sales { get; set; }
    }
}
