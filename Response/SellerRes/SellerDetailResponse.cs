namespace Respon.SellerRes
{
    public class SellerDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public decimal? Ratings { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? Province { get; set; }
        public int? ProvinceId { get; set; }
        public string? District { get; set; }
        public int? DistrictId { get; set; }
        public string? Ward { get; set; }
        public string? WardCode { get; set; }
        public string? Street { get; set; }
        public int Status { get; set; }
    }
}
