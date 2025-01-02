namespace Request
{
    public class FeedbackRequest
    {
        public int ProductId { get; set; }
        public double Ratings { get; set; }
        public string BuyerFeedback { get; set; }
    }
}
