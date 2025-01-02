using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement {
    public class AddressDAO : DataAccessBase<Address>, IAddressDAO {
        public AddressDAO(PlatformAntiquesHandicraftsContext context) : base(context) {
        }

        public Address Get(int id) {
            return GetAll().FirstOrDefault(p => p.Id == id);
        }

        public Address GetBuyerAddressInOrder(int buyerId, string address) {
            return GetAll().FirstOrDefault(p => p.CustomerId == buyerId && address.Contains(p.Street));
        }

        public IQueryable<Address> GetByCustomerId(int id) {
            return GetAll().Where(p => p.CustomerId == id);
        }

        public IQueryable<Address> GetDeliveryByCustomerId(int id)
        {
            return GetAll().Where(p => p.CustomerId == id && p.Type == (int)AddressType.Delivery);
        }

        public IQueryable<Address> GetPickupByCustomerId(int id)
        {
            return GetAll().Where(p => p.CustomerId == id && p.Type == (int)AddressType.Pickup);
        }

        public Address GetPickupBySellerId(int id)
        {
            return GetAll().Where(p => p.CustomerId == id && p.Type == (int)AddressType.Pickup).FirstOrDefault();
        }
    }
}
