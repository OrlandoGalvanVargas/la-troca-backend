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
        private readonly HashSet<string> _exactMatchProfanities;
        private readonly HashSet<string> _allowedNames;

        public TextModerationServices()
        {
            _detector = new ProfanityDetector(allLocales: true);

            // ✅ Solo groserías completas (no subcadenas)
            _exactMatchProfanities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Groserías completas en español
                "puto", "puta", "putos", "putas",
                "pendejo", "pendeja", "pendejos", "pendejas",
                "idiota", "imbecil", "tarado", "estupido", "estúpido",
                "baboso", "babosa", "inutil", "inútil",
                "cabron", "cabrona", "mierda", "chingado", "chingada",
                "chingar", "chingón", "chingona", "culero", "culera",
                "pinche", "zorra", "marica", "malparido",
                "bastardo", "perra", "perro", "tonto", "tonta",
                "payaso", "payasa", "mamón", "mamona",
                "verga", "vrga", "pito", "wey", "guey",
                
                // Groserías en inglés (pero NO incluimos "dick" porque es nombre común)
                "fuck", "shit", "bitch", "asshole", "pussy", "cunt",
                "motherfucker", "fucker", "damn", "bastard"
            };

            // ✅ Lista blanca de nombres comunes que NO deben bloquearse
            _allowedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Nombres en español
                "orlando", "gustavo", "sebastian", "cristian",
                "salvador", "jesus", "angel", "marco", "marcos",
                "arsenio", "kury", "javith", "centeno", "mucino",
                
                // Nombres en inglés que pueden ser confundidos
                "dick", "randy", "anders", "peter", "richard",
                "johnson", "wang", "cox", "ball", "peter"
            };
        }

        public async Task<bool> IsTextSafeAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            // 🔹 Normaliza el texto
            var normalized = NormalizeText(text);

            // 🔹 LOG para debugging
            Console.WriteLine($"🔍 MODERACIÓN DE TEXTO:");
            Console.WriteLine($"   Original: {text}");
            Console.WriteLine($"   Normalizado: {normalized}");

            // 🔹 Verifica si TODO el texto normalizado es un nombre permitido
            if (_allowedNames.Contains(normalized))
            {
                Console.WriteLine($"   ✅ PERMITIDO: Nombre completo en lista blanca");
                return true;
            }

            // 🔹 Divide en palabras
            var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 🔹 Verifica cada palabra COMPLETA
            foreach (var word in words)
            {
                Console.WriteLine($"   🔎 Analizando palabra: '{word}'");

                // Ignora iniciales (ej: "G", "V", "J")
                if (word.Length <= 1)
                {
                    Console.WriteLine($"      ↪️ Ignorada (inicial de 1 letra)");
                    continue;
                }

                // ✅ PRIMERO: Verifica si la palabra está en la lista blanca
                if (_allowedNames.Contains(word))
                {
                    Console.WriteLine($"      ✅ Permitida (en lista blanca)");
                    continue;
                }

                // Verifica si la palabra está en nuestra lista de groserías
                if (_exactMatchProfanities.Contains(word))
                {
                    Console.WriteLine($"   ❌ BLOQUEADO: '{word}' es una grosería exacta");
                    return false;
                }

                // Solo usamos el detector como apoyo secundario
                bool isProfane = await Task.Run(() => _detector.IsProfane(word));
                if (isProfane)
                {
                    Console.WriteLine($"      ⚠️ Detector marcó '{word}' como profanidad");

                    // Verificación adicional: palabras muy cortas suelen ser falsos positivos
                    if (word.Length <= 3)
                    {
                        Console.WriteLine($"      ↪️ Ignorada (palabra muy corta, probablemente falso positivo)");
                        continue;
                    }

                    Console.WriteLine($"   ❌ BLOQUEADO: '{word}' detectado como profanidad por detector externo");
                    return false;
                }

                Console.WriteLine($"      ✅ Palabra segura");
            }

            // 🔹 Verificación de groserías como frases completas
            if (words.Length > 1)
            {
                foreach (var badWord in _exactMatchProfanities)
                {
                    var pattern = $@"\b{Regex.Escape(badWord)}\b";
                    if (Regex.IsMatch(normalized, pattern, RegexOptions.IgnoreCase))
                    {
                        Console.WriteLine($"   ❌ BLOQUEADO: '{badWord}' encontrado como palabra completa en frase");
                        return false;
                    }
                }
            }

            Console.WriteLine($"   ✅ TEXTO SEGURO - Aprobado");
            return true;
        }

        private string NormalizeText(string input)
        {
            // Convierte a minúsculas
            input = input.ToLowerInvariant();

            // Elimina acentos (ñ se convierte en n, á en a, etc.)
            input = input.Normalize(NormalizationForm.FormD);
            input = new string(input
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            // Reemplaza puntos por espacios (para "G.V" → "G V")
            input = input.Replace(".", " ");

            // Elimina otros caracteres especiales pero mantiene espacios
            input = Regex.Replace(input, @"[^\w\s]", " ");

            // Normaliza espacios múltiples
            input = Regex.Replace(input, @"\s+", " ");

            return input.Trim();
        }
    }
}