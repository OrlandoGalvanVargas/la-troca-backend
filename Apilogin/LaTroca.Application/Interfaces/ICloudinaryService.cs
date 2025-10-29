using Microsoft.AspNetCore.Http;

namespace LaTroca.Application.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string publicId);
    }
}
