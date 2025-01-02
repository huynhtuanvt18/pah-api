using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public interface IWithdrawalDAO {
        public void Create(Withdrawal withdrawal);
        public void Update(Withdrawal withdrawal);
        public IQueryable<Withdrawal> GetByUserId(int userId);
        public IQueryable<Withdrawal> GetAll();
        public Withdrawal Get(int id);
    }
}
