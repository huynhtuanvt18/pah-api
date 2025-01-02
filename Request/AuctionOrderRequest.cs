using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Request {
    public class AuctionOrderRequest {
        [Required(ErrorMessage = "Không được để trống ID đấu giá")]
        [Range(1, int.MaxValue, ErrorMessage = "Điền vào giá trị lớn hơn {1}")]
        public int AuctionId {  get; set; }
        [Required(ErrorMessage = "Không được để trống giá ship")]
        [Range(0, int.MaxValue, ErrorMessage = "Điền vào giá trị lớn hơn {1}")]
        public decimal ShippingPrice { get; set; }
        [Required(ErrorMessage = "Không được để trống ID địa chỉ")]
        [Range(1, int.MaxValue, ErrorMessage = "Điền vào giá trị lớn hơn {1}")]
        public int AddressId { get; set; }
    }
}
