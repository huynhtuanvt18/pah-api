using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.EmailService {
    public interface IEmailService {
        public Task SendEmail(Message message);
    }
}
