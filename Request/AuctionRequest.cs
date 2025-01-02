namespace Request
{
    public class AuctionRequest
    {
        public string Title { get; set; } = null!;
        public decimal Step { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}
