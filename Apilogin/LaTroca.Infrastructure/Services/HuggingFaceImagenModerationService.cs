using LaTroca.Moderacion.Application.DTOs;
using LaTroca.Moderacion.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LaTroca.Moderacion.Infrastructure.Services
{
    public class HuggingFaceImagenModerationService : IImageModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public HuggingFaceImagenModerationService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["HuggingFace:ApiKey"] ?? throw new ArgumentNullException("HuggingFace:ApiKey");
        }

        public async Task<ModerationResultDto> AnalyzeImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new ModerationResultDto { IsSafe = false, Message = "No se proporcionó una imagen válida." };

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();

            using var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(
                "https://api-inference.huggingface.co/models/Falconsai/nsfw_image_detection", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new ModerationResultDto { IsSafe = false, Message = $"❌ Error: {json}" };

            using var doc = JsonDocument.Parse(json);
            var results = doc.RootElement.EnumerateArray().ToList();

            var labels = new List<(string Label, double Score)>();
            foreach (var item in results)
            {
                var label = item.GetProperty("label").GetString() ?? "unknown";
                var score = item.GetProperty("score").GetDouble();
                labels.Add((label, score));
            }

            // 🔹 Umbral muy bajo: incluso 1.2% no pasa
            var nsfw = labels.FirstOrDefault(l => l.Label.ToLower().Contains("nsfw"));
            bool isSafe = nsfw.Score < 0.01;

            string message = (isSafe ? "✅ Imagen segura.\n" : "⚠️ Imagen con posible contenido inapropiado.\n") +
                             string.Join("\n", labels.Select(l => $"{l.Label}: {(l.Score * 100):F1}%"));

            return new ModerationResultDto
            {
                IsSafe = isSafe,
                Message = message
            };
        }
    }
}
