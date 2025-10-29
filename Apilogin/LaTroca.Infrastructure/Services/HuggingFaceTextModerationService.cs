using LaTroca.Application.DTOs;
using LaTroca.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LaTroca.Infrastructure.Services
{
    public class HuggingFaceTextModerationService : ITextModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private const double ToxicThreshold = 0.08;
        private const double ObsceneThreshold = 0.05;
        private const double InsultThreshold = 0.05;
        private const double OffensiveThreshold = 0.35;

        public HuggingFaceTextModerationService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["HuggingFace:ApiKey"] ?? throw new ArgumentNullException("HuggingFace:ApiKey");
        }

        public async Task<ModerationResultDto> AnalyzeTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new ModerationResultDto
                {
                    IsSafe = true,
                    Message = "Texto seguro.",
                    RiskLevel = "safe"
                };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            string[] models =
            {
                "unitary/toxic-bert",
                "cardiffnlp/twitter-roberta-base-offensive"
            };

            bool isInappropriate = false;

            foreach (var model in models)
            {
                var body = new { inputs = text };
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"https://api-inference.huggingface.co/models/{model}", content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ModerationResultDto
                    {
                        IsSafe = false,
                        Message = "Texto inapropiado.",
                        RiskLevel = "unsafe"
                    };
                }

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) continue;

                var results = doc.RootElement[0].EnumerateArray()
                    .Select(x => new
                    {
                        Label = x.GetProperty("label").GetString(),
                        Score = x.GetProperty("score").GetDouble()
                    })
                    .ToList();

                if (model.Contains("toxic-bert"))
                {
                    double toxic = results.FirstOrDefault(r => r.Label.Contains("toxic", StringComparison.OrdinalIgnoreCase))?.Score ?? 0;
                    double obscene = results.FirstOrDefault(r => r.Label.Contains("obscene", StringComparison.OrdinalIgnoreCase))?.Score ?? 0;
                    double insult = results.FirstOrDefault(r => r.Label.Contains("insult", StringComparison.OrdinalIgnoreCase))?.Score ?? 0;

                    if (toxic >= ToxicThreshold || obscene >= ObsceneThreshold || insult >= InsultThreshold)
                        isInappropriate = true;
                }
                else if (model.Contains("twitter-roberta-base-offensive"))
                {
                    double offensive = results.FirstOrDefault(r => r.Label.Equals("offensive", StringComparison.OrdinalIgnoreCase))?.Score ?? 0;
                    if (offensive >= OffensiveThreshold)
                        isInappropriate = true;
                }
            }

            return new ModerationResultDto
            {
                IsSafe = !isInappropriate,
                Message = isInappropriate ? "Texto inapropiado." : "Texto seguro.",
                RiskLevel = isInappropriate ? "unsafe" : "safe"
            };
        }
    }
}
