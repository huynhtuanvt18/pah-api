using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Transaction
    {
        public int Id { get; set; }
        public int? WalletId { get; set; }
        public int? PaymentMethod { get; set; }
        public decimal? Amount { get; set; }
        public int Type { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }

        public virtual Wallet? Wallet { get; set; }
    }
}
