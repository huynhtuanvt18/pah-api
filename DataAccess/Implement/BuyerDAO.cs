using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement {
    public class BuyerDAO : DataAccessBase<Buyer>, IBuyerDAO {
        public BuyerDAO(PlatformAntiquesHandicraftsContext context) : base(context) {
        }

        public Buyer Get(int id) {
            return GetAll().FirstOrDefault(x => x.Id == id);
        }
    }
}
