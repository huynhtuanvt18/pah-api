using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Token
    {
        public int Id { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiryTime { get; set; }

        public virtual User IdNavigation { get; set; } = null!;
    }
}
