using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Wallet
    {
        public Wallet()
        {
            Transactions = new HashSet<Transaction>();
            Withdrawals = new HashSet<Withdrawal>();
        }

        public int Id { get; set; }
        public decimal? AvailableBalance { get; set; }
        public decimal? LockedBalance { get; set; }
        public int Status { get; set; }

        public virtual User IdNavigation { get; set; } = null!;
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<Withdrawal> Withdrawals { get; set; }
    }
}
