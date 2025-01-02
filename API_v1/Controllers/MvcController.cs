using Microsoft.AspNetCore.Mvc;

namespace API.Controllers {
    public class MvcController : Controller {
        [HttpGet("/mvc/verify")]
        public IActionResult Index() {
            return View("/Views/Mvc/Index.cshtml");
        }

        [HttpGet("/mvc/error")]
        public IActionResult Error()
        {
            return View("/Views/Mvc/Error.cshtml");
        }
    }
}
