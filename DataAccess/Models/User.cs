using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class User
    {
        public User()
        {
            Addresses = new HashSet<Address>();
            Auctions = new HashSet<Auction>();
            Withdrawals = new HashSet<Withdrawal>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Phone { get; set; }
        public string? ProfilePicture { get; set; }
        public int? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public int Role { get; set; }
        public int Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Buyer? Buyer { get; set; }
        public virtual Seller? Seller { get; set; }
        public virtual Token? Token { get; set; }
        public virtual VerifyToken? VerifyToken { get; set; }
        public virtual Wallet? Wallet { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Auction> Auctions { get; set; }
        public virtual ICollection<Withdrawal> Withdrawals { get; set; }
    }
}
