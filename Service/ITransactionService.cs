using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface ITransactionService
    {
        public List<Transaction> GetTransactionsByUserId(int userId, int type, int orderBy);
        public List<Transaction> GetAllTransactions();
        public Transaction GetTransactionById(int id);
    }
}
