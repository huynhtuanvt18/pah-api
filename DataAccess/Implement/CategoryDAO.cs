using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement
{
    public class CategoryDAO : DataAccessBase<Category>, ICategoryDAO
    {
        public CategoryDAO(PlatformAntiquesHandicraftsContext context) : base(context) { }

        IQueryable<Category> ICategoryDAO.GetAll()
        {
            return GetAll();
        }
    }
}
