using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public interface IProductDAO
    {
        public Product GetProductById(int id);
        public IQueryable<Product> GetProducts();
        public IQueryable<Product> GetProductsBySellerId(int sellerId);
        public void CreateProduct(Product product);
        public void UpdateProduct(Product product);
    }
}
