using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class ImageController : ControllerBase {
        private readonly IConfiguration _configuration;
        private readonly IImageService _imageService;
        public ImageController(IConfiguration configuration, IImageService imageService) {
            _configuration = configuration;
            _imageService = imageService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file) {
            if (file == null) {
                return BadRequest();
            }

            // Get any Stream - it can be FileStream, MemoryStream or any other type of Stream
            var stream = file.OpenReadStream();

            return Ok(new { url =  _imageService.StoreImageAsync(file.FileName, stream).Result});
        }
    }
}
