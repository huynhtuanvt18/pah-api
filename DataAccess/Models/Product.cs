using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Product
    {
        public Product()
        {
            Auctions = new HashSet<Auction>();
            Feedbacks = new HashSet<Feedback>();
            OrderItems = new HashSet<OrderItem>();
            ProductImages = new HashSet<ProductImage>();
        }

        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public int? MaterialId { get; set; }
        public int? SellerId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Dimension { get; set; }
        public decimal? Weight { get; set; }
        public string? Origin { get; set; }
        public string? PackageMethod { get; set; }
        public string? PackageContent { get; set; }
        public int Condition { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public decimal? Ratings { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Material? Material { get; set; }
        public virtual Seller? Seller { get; set; }
        public virtual ICollection<Auction> Auctions { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
    }
}
