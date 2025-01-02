using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionDAO _transactionDAO;

        public TransactionService(ITransactionDAO transactionDAO)
        {
            _transactionDAO = transactionDAO;
        }

        public List<Transaction> GetAllTransactions()
        {
            return _transactionDAO.GetAll().ToList();
        }

        public Transaction GetTransactionById(int id)
        {
            return _transactionDAO.GetById(id);
        }

        public List<Transaction> GetTransactionsByUserId(int userId, int type, int orderBy)
        {
            var transactionList = _transactionDAO.GetByUserId(userId).Where(t => type == 0 || t.Type == type);

            switch (orderBy)
            {
                case 1:
                    transactionList = transactionList.OrderBy(t => t.Date);
                    break;
                case 2:
                    transactionList = transactionList.OrderBy(t => t.Amount);
                    break;
                case 3:
                    transactionList = transactionList.OrderByDescending(t => t.Amount);
                    break;
                default:
                    transactionList = transactionList.OrderByDescending(t => t.Date);
                    break;
            }
            return transactionList.ToList();
        }
    }
}
