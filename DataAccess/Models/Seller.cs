using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Seller
    {
        public Seller()
        {
            Orders = new HashSet<Order>();
            Products = new HashSet<Product>();
            Responses = new HashSet<Response>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public decimal? Ratings { get; set; }
        public int Status { get; set; }
        public string ShopId { get; set; } = null!;

        public virtual User IdNavigation { get; set; } = null!;
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Response> Responses { get; set; }
    }
}
