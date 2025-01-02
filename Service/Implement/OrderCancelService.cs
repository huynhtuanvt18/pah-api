using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class OrderCancelService : IOrderCancelService
    {
        private readonly IOrderCancelDAO _orderCancelDAO;

        public OrderCancelService(IOrderCancelDAO orderCancelDAO)
        {
            _orderCancelDAO = orderCancelDAO;
        }

        public OrderCancellation Get(int id)
        {
            return _orderCancelDAO.Get(id);
        }
    }
}
