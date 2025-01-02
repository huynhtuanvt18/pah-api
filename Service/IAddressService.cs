using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service {
    public interface IAddressService {
        public List<Address> GetByCustomerId(int customerId);
        public Address Get(int addressId);
        public Address GetPickupBySellerId(int sellerId);
        public Address GetDeliveryByCurrentUser(int id);
        public void Create(Address address);
        public void Update(Address address, int customerId);
        public void UpdateSellerAddress(Address address, int customerId);
        public void Delete(int addressId, int customerId);
    }
}
