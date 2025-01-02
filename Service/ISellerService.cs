using DataAccess.Models;
using Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface ISellerService
    {
        public Seller GetSeller(int id);
        public List<Seller> GetSellerRequestList();
        public List<Seller> GetSellers();
        public int CreateSeller(int id, Seller seller);
        public void UpdateSeller(Seller seller);
        public decimal GetSalesCurrentSeller(int id);
        public decimal GetSalesCurrentSellerAllTime(int id);
        public List<Order> GetOrdersThreeMonthsCurrentSeller(int id);

        public Task<int> CreateShopIdAsync(SellerRequest sellerRequest);
    }
}
