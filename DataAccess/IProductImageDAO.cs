using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public interface IProductImageDAO {
        public void Create(ProductImage productImage);
        public void Update(ProductImage productImage);

        public ProductImage Get(int id);
        public List<ProductImage> GetByProductId(int productId);
    }
}
