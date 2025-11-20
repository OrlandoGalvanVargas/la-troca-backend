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

        public HuggingFaceTextModerationService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["HuggingFace:ApiKey"] ?? throw new ArgumentNullException("HuggingFace:ApiKey");
        }

        public async Task<ModerationResultDto> AnalyzeTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new ModerationResultDto { IsSafe = true, Message = "Texto vacío o nulo." };

            Console.WriteLine($"\n🤖 === ANÁLISIS HUGGINGFACE ===");
            Console.WriteLine($"📝 Texto a analizar: '{text}'");

            string[] models =
            {
                "unitary/toxic-bert",
                "eliasalbouzidi/distilbert-nsfw-text-classifier"
            };

            double toxicScore = 0.0;
            double obsceneScore = 0.0;
            double insultScore = 0.0;
            double identityHateScore = 0.0;
            double severeToxicScore = 0.0;
            double threatScore = 0.0;
            double nsfwScore = 0.0;
            string details = "";

            int wordCount = CountWords(text);
            Console.WriteLine($"📊 Conteo de palabras: {wordCount}");

            foreach (var model in models)
            {
                var body = new { inputs = text };
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                var response = await _httpClient.PostAsync($"https://router.huggingface.co/hf-inference/models/{model}", content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error con modelo {model}: {json}");
                    return new ModerationResultDto
                    {
                        IsSafe = false,
                        Message = $"❌ Error al analizar texto con {model}. Respuesta del servidor: {json}"
                    };
                }

                using var doc = JsonDocument.Parse(json);
                var resultsList = new List<(string Label, double Score)>();

                void Extract(JsonElement el)
                {
                    if (el.ValueKind == JsonValueKind.Object)
                    {
                        if (el.TryGetProperty("label", out var label) && el.TryGetProperty("score", out var score))
                        {
                            resultsList.Add((label.GetString() ?? "unknown", score.GetDouble()));
                        }
                    }
                    else if (el.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var i in el.EnumerateArray()) Extract(i);
                    }
                }
                Extract(doc.RootElement);

                details += $"🔹 {model} → {string.Join(", ", resultsList.Select(r => $"{r.Label}: {(r.Score * 100):F1}%"))}\n";
                Console.WriteLine($"🔹 {model}:");
                foreach (var r in resultsList)
                {
                    Console.WriteLine($"   - {r.Label}: {(r.Score * 100):F2}%");
                }

                if (model.Contains("toxic"))
                {
                    toxicScore = resultsList.FirstOrDefault(r => r.Label.Equals("toxic", StringComparison.OrdinalIgnoreCase)).Score;
                    obsceneScore = resultsList.FirstOrDefault(r => r.Label.Equals("obscene", StringComparison.OrdinalIgnoreCase)).Score;
                    insultScore = resultsList.FirstOrDefault(r => r.Label.Equals("insult", StringComparison.OrdinalIgnoreCase)).Score;
                    identityHateScore = resultsList.FirstOrDefault(r => r.Label.Equals("identity_hate", StringComparison.OrdinalIgnoreCase)).Score;
                    severeToxicScore = resultsList.FirstOrDefault(r => r.Label.Equals("severe_toxic", StringComparison.OrdinalIgnoreCase)).Score;
                    threatScore = resultsList.FirstOrDefault(r => r.Label.Equals("threat", StringComparison.OrdinalIgnoreCase)).Score;
                }
                if (model.Contains("nsfw"))
                {
                    var nsfwCandidate = resultsList.FirstOrDefault(r =>
                        r.Label.Equals("nsfw", StringComparison.OrdinalIgnoreCase) ||
                        r.Label.Equals("unsafe", StringComparison.OrdinalIgnoreCase) ||
                        r.Label.IndexOf("sex", StringComparison.OrdinalIgnoreCase) >= 0);

                    nsfwScore = nsfwCandidate.Score;
                }
            }

            bool isSafe = true;
            string category = "Normal";

            // ✅ UMBRALES AJUSTADOS - Más conservadores para evitar falsos positivos
            const double TOXIC_OFFENSIVE_THRESHOLD = 0.70;  // 70% (antes: 25%)
            const double OBSCENE_THRESHOLD = 0.65;          // 65% (antes: 15%)
            const double INSULT_THRESHOLD = 0.60;           // 60% (antes: 15%)
            const double IDENTITY_HATE_THRESHOLD = 0.65;    // 65% (antes: 15%)
            const double SEVERE_TOXIC_THRESHOLD = 0.50;     // 50% (antes: 10%)
            const double THREAT_THRESHOLD = 0.60;           // 60% (antes: 10%)

            const double NSFW_STRONG_THRESHOLD = 0.99;      // Sin cambio
            const double NSFW_MID_THRESHOLD = 0.85;         // 85% (antes: 55%)
            const double NSFW_WEAK_THRESHOLD = 0.70;        // 70% (antes: 30%)
            const int NSFW_WEAK_WORDCOUNT_MIN = 5;          // 5 palabras (antes: 3)

            Console.WriteLine($"\n📊 SCORES FINALES:");
            Console.WriteLine($"   Toxic: {(toxicScore * 100):F2}% (umbral: {(TOXIC_OFFENSIVE_THRESHOLD * 100):F0}%)");
            Console.WriteLine($"   Obscene: {(obsceneScore * 100):F2}% (umbral: {(OBSCENE_THRESHOLD * 100):F0}%)");
            Console.WriteLine($"   Insult: {(insultScore * 100):F2}% (umbral: {(INSULT_THRESHOLD * 100):F0}%)");
            Console.WriteLine($"   Identity Hate: {(identityHateScore * 100):F2}% (umbral: {(IDENTITY_HATE_THRESHOLD * 100):F0}%)");
            Console.WriteLine($"   Severe Toxic: {(severeToxicScore * 100):F2}% (umbral: {(SEVERE_TOXIC_THRESHOLD * 100):F0}%)");
            Console.WriteLine($"   Threat: {(threatScore * 100):F2}% (umbral: {(THREAT_THRESHOLD * 100):F0}%)");
            Console.WriteLine($"   NSFW: {(nsfwScore * 100):F2}% (umbrales: {(NSFW_STRONG_THRESHOLD * 100):F0}%/{(NSFW_MID_THRESHOLD * 100):F0}%/{(NSFW_WEAK_THRESHOLD * 100):F0}%)");

            if (toxicScore >= TOXIC_OFFENSIVE_THRESHOLD ||
                obsceneScore >= OBSCENE_THRESHOLD ||
                insultScore >= INSULT_THRESHOLD ||
                identityHateScore >= IDENTITY_HATE_THRESHOLD ||
                severeToxicScore >= SEVERE_TOXIC_THRESHOLD ||
                threatScore >= THREAT_THRESHOLD)
            {
                isSafe = false;
                category = "Ofensivo";
                Console.WriteLine($"❌ BLOQUEADO: Categoría {category}");
            }
            else
            {
                // Solo bloquea NSFW si el score es MUY alto
                if (nsfwScore >= NSFW_STRONG_THRESHOLD && wordCount >= 3)
                {
                    isSafe = false;
                    category = "Sexual";
                    Console.WriteLine($"❌ BLOQUEADO: NSFW Strong");
                }
                else if (nsfwScore >= NSFW_MID_THRESHOLD && wordCount >= 4)
                {
                    isSafe = false;
                    category = "Sexual";
                    Console.WriteLine($"❌ BLOQUEADO: NSFW Mid");
                }
                else if (nsfwScore >= NSFW_WEAK_THRESHOLD && wordCount >= NSFW_WEAK_WORDCOUNT_MIN)
                {
                    isSafe = false;
                    category = "Sexual";
                    Console.WriteLine($"❌ BLOQUEADO: NSFW Weak");
                }
                else
                {
                    Console.WriteLine($"✅ APROBADO: Texto seguro");
                }
            }

            string message = isSafe
                ? $"✅ Texto seguro"
                : $"❌ Texto inapropiado ({category})";

            Console.WriteLine($"🏁 RESULTADO FINAL: {message}\n");

            return new ModerationResultDto
            {
                IsSafe = isSafe,
                Message = message
            };
        }

        private int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            var tokens = text.Trim().Split(new char[] { ' ', '\t', '\n', '\r', ',', '.', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            return tokens.Length;
        }
    }
}