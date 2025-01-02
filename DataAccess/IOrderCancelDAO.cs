using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public interface IOrderCancelDAO {
        public OrderCancellation Get(int id);
        public void Create(OrderCancellation orderCancellation);
    }
}
