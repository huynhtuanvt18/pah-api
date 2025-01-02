using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public interface IAddressDAO {
        public void Create(Address address);
        public void Update(Address address);
        public void Delete(Address address);
        public Address Get(int id);
        public Address GetPickupBySellerId(int id);

        public IQueryable<Address> GetByCustomerId(int id);
        public IQueryable<Address> GetPickupByCustomerId(int id);
        public IQueryable<Address> GetDeliveryByCustomerId(int id);
        public Address GetBuyerAddressInOrder(int buyerId, string address);
    }
}
