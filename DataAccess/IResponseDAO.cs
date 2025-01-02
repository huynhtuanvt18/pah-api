using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public interface IResponseDAO
    {
        public Response GetByFeedbackId(int feedbackId);
        public void CreateResponse(Response response);
    }
}
