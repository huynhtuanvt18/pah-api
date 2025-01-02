using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respon.UserRes
{
    public class BuyerWithOrderNumberResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public int Role { get; set; }
        public int Status { get; set; }
        public int NumberOfDoneOrders { get; set; }
    }
}
