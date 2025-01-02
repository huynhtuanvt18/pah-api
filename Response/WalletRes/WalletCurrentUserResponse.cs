namespace Respon.WalletRes
{
    public class WalletCurrentUserResponse
    {
        public int Id { get; set; }
        public decimal? AvailableBalance { get; set; }
        public decimal? LockedBalance { get; set; }
    }
}
