using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class VerifyToken
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public DateTime ExpirationDate { get; set; }
        public int Status { get; set; }

        public virtual User IdNavigation { get; set; } = null!;
    }
}
