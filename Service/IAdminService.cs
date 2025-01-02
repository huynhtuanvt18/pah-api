using DataAccess.Models;
using Request;
using Request.Param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service {
    public interface IAdminService {
        public void CreateStaff(User user);
        public void UpdateStaff(User user);
        public List<User> GetAccounts(AccountParam accountParam);
        public void UpdateStatusAccount(int id, int status);
    }
}
