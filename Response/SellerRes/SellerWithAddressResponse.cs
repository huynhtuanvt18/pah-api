namespace Respon.SellerRes
{
    public class SellerWithAddressResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ProfilePicture { get; set; }
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
