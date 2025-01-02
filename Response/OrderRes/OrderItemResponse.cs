namespace Respon.OrderRes {
    public class OrderItemResponse {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public string ProductName { get; set; }
        public int ProductType { get; set; }
    }
}
