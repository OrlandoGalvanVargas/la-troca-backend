using LaTroca.Application.Interfaces;
using DotnetBadWordDetector;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LaTroca.Application.Services
{
    public class TextModerationServices : ITextModerationServices
    {
        private readonly ProfanityDetector _detector;
        private readonly HashSet<string> _customProfanities;

        public TextModerationServices()
        {
            // ✅ Activamos todos los idiomas soportados
            _detector = new ProfanityDetector(allLocales: true);

            // ✅ Creamos nuestra lista personalizada de groserías comunes en español
            _customProfanities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "puto", "puta", "putos", "putas",
                "pendejo", "pendeja", "pendejos", "pendejas",
                "idiota", "imbecil", "tarado", "estupido", "estúpido",
                "baboso", "babosa", "inutil", "inútil",
                "cabron", "cabrona", "mierda", "chingado", "chingada",
                "chingar", "chingón", "chingona", "culero", "culera",
                "pinche", "zorra", "marica", "malparido",
                "bastardo", "perra", "perro", "tonto", "tonta","pito","wey",
                "payaso", "payasa", "mamón", "mamona"
            };
        }

        public async Task<bool> IsTextSafeAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            // 🔹 Normaliza el texto (acentos, puntuación, etc.)
            var normalized = NormalizeText(text);

            // 🔹 Divide en palabras individuales
            var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 🔹 Verifica palabra por palabra (primero con el detector)
            foreach (var word in words)
            {
                bool isProfane = await Task.Run(() => _detector.IsProfane(word));

                // Si el detector o la lista personalizada lo consideran grosería → bloqueo
                if (isProfane || _customProfanities.Contains(word))
                    return false;
            }

            // 🔹 También verifica el texto completo (por si está en medio de una frase)
            bool fullTextProfane = await Task.Run(() => _detector.IsProfane(normalized));

            if (fullTextProfane)
                return false;

            // 🔹 Verificación adicional con lista personalizada (por si hay frases)
            foreach (var badWord in _customProfanities)
            {
                if (normalized.Contains(badWord, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private string NormalizeText(string input)
        {
            // Convierte a minúsculas
            input = input.ToLowerInvariant();

            // Elimina acentos
            input = input.Normalize(NormalizationForm.FormD);
            input = new string(input
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            // Elimina caracteres especiales y puntuación
            input = Regex.Replace(input, @"[^\w\s]", " ");

            // 🔹 No quitamos plurales
            return input.Trim();
        }
    }
}
