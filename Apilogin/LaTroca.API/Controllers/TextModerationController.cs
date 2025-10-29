using LaTroca.Application.Interfaces;
using LaTroca.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaTroca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TextModerationController : ControllerBase
    {
        private readonly ITextModerationService _textModerationService;

        public TextModerationController(ITextModerationService textModerationService)
        {
            _textModerationService = textModerationService;
        }

        [HttpPost("AnalizarTexto")]
        public async Task<IActionResult> AnalyzeText([FromBody] TextModerationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Descripcion))
                return BadRequest("Debe proporcionar una descripción válida para analizar.");

            var result = await _textModerationService.AnalyzeTextAsync(request.Descripcion);
            return Ok(result);
        }
    }
}
