using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class OrderCancellation
    {
        public int Id { get; set; }
        public string? Reason { get; set; }

        public virtual Order IdNavigation { get; set; } = null!;
    }
}
