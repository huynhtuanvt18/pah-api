using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respon.UserRes
{
    public class StaffListCountResponse
    {
        public int Count { get; set; }
        public List<StaffResponse> StaffList { get; set; }
    }
}
