using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement
{
    public class MaterialDAO : DataAccessBase<Material>, IMaterialDAO
    {
        public MaterialDAO(PlatformAntiquesHandicraftsContext context) : base(context){}

        IQueryable<Material> IMaterialDAO.GetAll()
        {
            return GetAll();
        }
    }
}
