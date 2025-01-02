using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public interface IFeedbackDAO
    {
        public Feedback GetById(int id);
        public IQueryable<Feedback> GetAll(int productId);
        public void CreateFeedback(Feedback feedback);
        public void Update(Feedback feedback);
        public void Delete(Feedback feedback);
    }
}
