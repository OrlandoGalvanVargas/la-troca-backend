using LaTroca.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace LaTroca.Application.Interfaces
{
    public interface IImageModerationService
    {
        Task<ModerationResultDto> AnalyzeImageAsync(IFormFile file);
    }
    
}
