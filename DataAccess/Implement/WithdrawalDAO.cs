using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement {
    public class WithdrawalDAO : DataAccessBase<Withdrawal>, IWithdrawalDAO {
        public WithdrawalDAO(PlatformAntiquesHandicraftsContext context) : base(context) {
        }

        public Withdrawal Get(int id) {
            return GetAll().FirstOrDefault(p => p.Id == id);
        }

        public IQueryable<Withdrawal> GetByUserId(int userId) {
            return GetAll().Where(p => p.WalletId == userId);
        }
    }
}
