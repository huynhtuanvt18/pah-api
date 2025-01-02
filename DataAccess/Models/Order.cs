using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        public int Id { get; set; }
        public int? BuyerId { get; set; }
        public int? SellerId { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? RecipientAddress { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? OrderShippingCode { get; set; }
        public decimal? ShippingCost { get; set; }
        public int Status { get; set; }

        public virtual Buyer? Buyer { get; set; }
        public virtual Seller? Seller { get; set; }
        public virtual OrderCancellation? OrderCancellation { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
