using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respon.UserRes
{
    public class UserListCountResponse
    {
        public int Count { get; set; }
        public List<UserResponse> UserList { get; set; }
    }
}
