using LaTroca.Application.Interfaces;
using LaTroca.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaTroca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagenModerationController : ControllerBase
    {
        private readonly IImageModerationService _moderationService;

        public ImagenModerationController(IImageModerationService moderationService)
        {
            _moderationService = moderationService;
        }


        [HttpPost("AnalizarImagen")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AnalyzeImage([FromForm] ImagenModerationRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("Debe subir una imagen válida.");

            var result = await _moderationService.AnalyzeImageAsync(request.File);
            return Ok(result);
        }
    }
}
