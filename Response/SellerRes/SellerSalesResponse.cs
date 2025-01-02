using Respon.OrderRes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respon.SellerRes
{
    public class SellerSalesResponse
    {
        public decimal TotalSales { get; set; }
        public int SellingProduct {  get; set; }
        public int ProcessingOrders { get; set; }
        public int DoneOrders { get; set; }
        public int TotalOrders { get; set; }
        public int TotalAuctions { get; set; }
    }
}
