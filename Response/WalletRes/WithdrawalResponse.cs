using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respon.WalletRes {
    public class WithdrawalResponse {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public int? ManagerId { get; set; }
        public decimal Amount { get; set; }
        public string Bank { get; set; }
        public string BankNumber { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Status { get; set; }
        public string ManagerName { get; set; }
    }
}
