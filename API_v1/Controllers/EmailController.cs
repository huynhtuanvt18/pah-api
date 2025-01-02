using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using Service.EmailService;

namespace API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase {
        private readonly Service.EmailService.IEmailService _emailService;

        public EmailController(Service.EmailService.IEmailService emailService) {
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Send() {
            var message = new Message(new string[] { "baro150902@gmail.com" }, "Aloooo", "ALOOOOOOOOO");
            _emailService.SendEmail(message);

            return Ok();
        }
    }
}
