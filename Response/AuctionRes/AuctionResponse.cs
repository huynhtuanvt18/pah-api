namespace Respon.AuctionRes
{
    public class AuctionResponse
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string StaffName { get; set; }
        public string Title { get; set; } = null!;
        public decimal EntryFee { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal Step { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? RegistrationStart { get; set; }
        public DateTime? RegistrationEnd { get; set; }

        public ProductRes.ProductResponse Product { get; set; }
    }
}
