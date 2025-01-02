using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class ActivationRequest
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public DateTime? Timestamp { get; set; }
        public string? Content { get; set; }
        public int? Status { get; set; }
        public int? StaffId { get; set; }
    }
}
