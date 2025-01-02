using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public interface IOrderDAO {
        public Order Get(int id);
        public Order GetByProductId(int productId);
        public IQueryable<Order> GetAllOrder();

        public IQueryable<Order> GetAllByBuyerId(int id);
        public IQueryable<Order> GetAllBySellerId(int id);
        public IQueryable<Order> GetAllByBuyerIdAfterCheckout(int buyerId, DateTime now);

        public void Create(Order order);
        public Order UpdateOrder(Order order);
    }
}
