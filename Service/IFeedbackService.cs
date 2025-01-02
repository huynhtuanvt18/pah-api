using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IFeedbackService
    {
        public Feedback GetById(int id);
        public List<Feedback> GetAll(int productId);
        public List<Feedback> GetTop3Newest(int productId);
        public void CreateFeedback(int userId, int productId, string buyerFeedBack, double ratings);
        //public void Update(Feedback feedback);
        //public void Delete(Feedback feedback);
    }
}
