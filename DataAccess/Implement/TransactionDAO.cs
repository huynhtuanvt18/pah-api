using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement {
    public class TransactionDAO : DataAccessBase<Transaction>, ITransactionDAO {
        public TransactionDAO(PlatformAntiquesHandicraftsContext context) : base(context) {
        }

        public IQueryable<Transaction> GetByUserId(int userId) {
            return GetAll().Where(p => p.WalletId == userId);
        }

        public bool IsZalopayOrderValid(string appTransId, string mac) {
            var list = GetAll().Where(p => p.Description.Contains(appTransId) || p.Description.Contains(mac));
            return list.Count() == 0;
        }

        public IQueryable<Transaction> GetAllTransactions() {
            return GetAll();
        }

        public Transaction GetById(int id)
        {
            return GetAll().Where(t => t.Id == id).FirstOrDefault();
        }
    }
}
