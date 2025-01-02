using DataAccess.Models;
using Respon.UserRes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respon.OrderRes
{
    public class OrderDetailResponse
    {
        public int Id { get; set; }
        public int? BuyerId { get; set; }
        public int? SellerId { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? RecipientAddress { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? ShippingCost { get; set; }
        public string? OrderShippingCode { get; set; }
        public int Status { get; set; }
        public OrderCancellationResponse OrderCancellation { get; set; }

        public virtual SellerResponse? Seller { get; set; }
        public virtual ICollection<OrderItemResponse> OrderItems { get; set; }
    }
}
