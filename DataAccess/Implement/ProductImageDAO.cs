using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement {
    public class ProductImageDAO : DataAccessBase<ProductImage>, IProductImageDAO {
        public ProductImageDAO(PlatformAntiquesHandicraftsContext context) : base(context) {
        }

        public ProductImage Get(int id) {
            return GetAll().FirstOrDefault(p => p.Id == id && p.Status == (int) Status.Available);
        }

        public List<ProductImage> GetByProductId(int productId) {
            return GetAll().Where(p => p.ProductId == productId).ToList();
        }
    }
}
