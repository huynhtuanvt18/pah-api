using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryDAO _categoryDAO;

        public CategoryService(ICategoryDAO categoryDAO)
        {
            _categoryDAO = categoryDAO;
        }

        public List<Category> GetAll()
        {
            return _categoryDAO.GetAll().ToList();
        }
    }
}
