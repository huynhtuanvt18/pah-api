using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public interface IBuyerDAO {
        public void Create(Buyer buyer);
        public void Update(Buyer buyer);
        public Buyer Get(int id);
    }
}
