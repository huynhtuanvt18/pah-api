namespace Respon.ProductRes
{
    public class ProductListResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal? Ratings { get; set; }
        public string SellerName { get; set; }
        public string ImageUrl { get; set; }
    }
}
