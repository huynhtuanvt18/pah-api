using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public interface IWalletDAO {
        public void Create(Wallet wallet);
        public void Update(Wallet wallet);
        public Wallet GetByCurrentUser(int id);
        public Wallet Get(int userId);
        public Wallet GetWithoutStatus(int userId);
    }
}
