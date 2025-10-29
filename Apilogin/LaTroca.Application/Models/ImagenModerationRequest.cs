using Microsoft.AspNetCore.Http;

namespace LaTroca.Application.Models
{
    public class ImagenModerationRequest
    {

        public IFormFile File { get; set; } = default!;
    }
}
