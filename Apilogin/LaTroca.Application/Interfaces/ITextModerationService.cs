using LaTroca.Application.DTOs;
using TorneoUniversitario.Application.DTOs;

namespace LaTroca.Application.Interfaces
{
    public interface ITextModerationService
    {
        Task<ModerationResultDto> AnalyzeTextAsync(string text);
     
    }
}
