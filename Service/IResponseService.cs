using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IResponseService
    {
        public Response GetByFeedbackId(int feedbackId);
        public void Reply(Response response);
    }
}
