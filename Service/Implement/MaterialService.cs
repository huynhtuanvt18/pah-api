using DataAccess;
using DataAccess.Implement;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialDAO _materialDAO;

        public MaterialService(IMaterialDAO materialDAO)
        {
            _materialDAO = materialDAO;
        }

        public List<Material> GetAll()
        {
            return _materialDAO.GetAll().ToList();
        }
    }
}
