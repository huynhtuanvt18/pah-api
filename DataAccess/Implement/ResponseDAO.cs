using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement
{
    public class ResponseDAO : DataAccessBase<Response>, IResponseDAO
    {
        public ResponseDAO(PlatformAntiquesHandicraftsContext context): base(context) { }

        public Response GetByFeedbackId(int feedbackId)
        {
            return GetAll().FirstOrDefault(f => f.FeedbackId == feedbackId);
        }

        public void CreateResponse(Response response)
        {
            Create(response);
        }
    }
}
