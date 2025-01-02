using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Withdrawal
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public int? ManagerId { get; set; }
        public decimal Amount { get; set; }
        public string Bank { get; set; } = null!;
        public string BankNumber { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Status { get; set; }

        public virtual User? Manager { get; set; }
        public virtual Wallet Wallet { get; set; } = null!;
    }
}
