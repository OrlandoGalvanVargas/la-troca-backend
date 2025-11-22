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
        private readonly HashSet<string> _allowedWords; // 👈 CAMBIO: Ahora es "palabras permitidas" no solo nombres

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
                "joto", "jota", "pene", "vagina", "concha",
                
                // Groserías en inglés
                "fuck", "shit", "bitch", "asshole", "pussy", "cunt",
                "motherfucker", "fucker", "damn", "bastard", "whore"
            };

            // ✅ Lista blanca: Palabras legítimas que el detector puede marcar incorrectamente
            _allowedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Nombres comunes
                "orlando", "gustavo", "sebastian", "cristian",
                "salvador", "jesus", "angel", "marco", "marcos",
                "arsenio", "kury", "javith", "centeno", "mucino",
                "dick", "randy", "anders", "peter", "richard",
                "johnson", "wang", "cox", "ball",
                
                // Palabras comunes en español que pueden ser detectadas incorrectamente
                "mujer", "mujeres", "hombre", "hombres",
                "empoderada", "empoderado", "empoderamiento",
                "sexo", "sexual", "sexualidad", // contexto educativo/identidad
                "género", "genero", "trans", "transexual", "transgénero",
                "gay", "lesbiana", "homosexual", "bisexual", "lgbtq",
                "feminismo", "feminista", "machismo", "machista",
                "aborto", "embarazo", "menstruación", "menstrual",
                "mama", "mamá", "papa", "papá", "madre", "padre",
                "negro", "negra", "blanco", "blanca", // colores/razas en contexto apropiado
                "gorda", "gordo", "flaco", "flaca", "delgado", "delgada",
                
                // Palabras en inglés comunes
                "woman", "women", "man", "men", "girl", "boy",
                "gender", "sex", "sexuality", "sexual",
                "gay", "lesbian", "bisexual", "transgender",
                "black", "white", "brown", "asian",
                "fat", "thin", "skinny", "thick",
                
                // Palabras relacionadas con trueque/comercio
                "intercambio", "trueque", "cambio", "permuta",
                "vender", "comprar", "precio", "gratis", "dinero"
            };
        }

        public async Task<bool> IsTextSafeAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            // 🔹 Normaliza el texto
            var normalized = NormalizeText(text);

            // 🔹 LOG para debugging
            Console.WriteLine($"\n🔍 === MODERACIÓN DE TEXTO ===");
            Console.WriteLine($"   📝 Original: {text}");
            Console.WriteLine($"   🔄 Normalizado: {normalized}");

            // 🔹 Verifica si TODO el texto normalizado es una palabra permitida
            if (_allowedWords.Contains(normalized))
            {
                Console.WriteLine($"   ✅ PERMITIDO: Texto completo en lista blanca");
                return true;
            }

            // 🔹 Divide en palabras
            var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine($"   📊 Total palabras: {words.Length}");

            // 🔹 Verifica cada palabra COMPLETA
            foreach (var word in words)
            {
                Console.WriteLine($"\n   🔎 Analizando: '{word}'");

                // Ignora iniciales, números, palabras muy cortas
                if (word.Length <= 1 || int.TryParse(word, out _))
                {
                    Console.WriteLine($"      ↪️ Ignorada (inicial/número)");
                    continue;
                }

                // ✅ PRIMERO: Verifica si la palabra está en la lista blanca
                if (_allowedWords.Contains(word))
                {
                    Console.WriteLine($"      ✅ Permitida (lista blanca)");
                    continue;
                }

                // ❌ SEGUNDO: Verifica si es una grosería exacta
                if (_exactMatchProfanities.Contains(word))
                {
                    Console.WriteLine($"   ❌ BLOQUEADO: '{word}' es grosería exacta");
                    return false;
                }

                // ⚠️ TERCERO: Usa el detector como última verificación
                bool isProfane = await Task.Run(() => _detector.IsProfane(word));
                if (isProfane)
                {
                    Console.WriteLine($"      ⚠️ Detector marcó '{word}' como profanidad");

                    // Palabras cortas (3 letras o menos) son casi siempre falsos positivos
                    if (word.Length <= 3)
                    {
                        Console.WriteLine($"      ↪️ Ignorada (muy corta, falso positivo probable)");
                        continue;
                    }

                    // Palabras entre 4-5 letras: verificar si son comunes antes de bloquear
                    if (word.Length <= 5)
                    {
                        Console.WriteLine($"      ⚠️ Palabra corta marcada - revisión manual recomendada");
                        Console.WriteLine($"      ↪️ Permitida por ahora (palabra corta, posible falso positivo)");
                        continue;
                    }

                    // Solo bloqueamos si es una palabra larga y el detector está seguro
                    Console.WriteLine($"   ❌ BLOQUEADO: '{word}' detectada como profanidad (palabra larga)");
                    return false;
                }

                Console.WriteLine($"      ✅ Palabra segura");
            }

            // 🔹 Verificación de groserías como frases completas
            if (words.Length > 1)
            {
                Console.WriteLine($"\n   🔍 Verificando frases completas...");
                foreach (var badWord in _exactMatchProfanities)
                {
                    var pattern = $@"\b{Regex.Escape(badWord)}\b";
                    if (Regex.IsMatch(normalized, pattern, RegexOptions.IgnoreCase))
                    {
                        Console.WriteLine($"   ❌ BLOQUEADO: '{badWord}' encontrado como palabra completa");
                        return false;
                    }
                }
            }

            Console.WriteLine($"\n   ✅ RESULTADO: TEXTO SEGURO - Aprobado\n");
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