namespace Respon.ProductRes
{
    public class ProductDetailResponse
    {
        public int Id { get; set; }
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
        public decimal? Ratings { get; set; }
        public int? CategoryId { get; set; }
        public int? MaterialId { get; set; }
        public string CategoryName { get; set; }
        public string MaterialName { get; set; }
        public string SellerName { get; set; }
        public List<string> ImageUrls { get; set; }
        public SellerRes.SellerWithAddressResponse Seller { get; set; }
        public List<FeedbackRes.FeedbackResponse> Feedbacks {  get; set; } 
    }
}
