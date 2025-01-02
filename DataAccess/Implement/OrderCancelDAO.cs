using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement {
    public class OrderCancelDAO : DataAccessBase<OrderCancellation>, IOrderCancelDAO {
        public OrderCancelDAO(PlatformAntiquesHandicraftsContext context) : base(context) {
        }

        public OrderCancellation Get(int id) {
            return GetAll().FirstOrDefault(p => p.Id == id);
        }
    }
}
