using AkanjiApp.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace AkanjiApp.Services
{
    public class PdfService
    {

        private readonly HttpClient _httpClient;

        public PdfService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public string ExtractText(string filePath)
        {
            StringBuilder texto = new StringBuilder();

            using (PdfDocument pdf = PdfDocument.Open(filePath))
            {
                foreach (var pagina in pdf.GetPages())
                {
                    texto.AppendLine(ContentOrderTextExtractor.GetText(pagina));
                }
            }

            return texto.ToString();
        }

        public string FindAcknowledgement(string texto)
        {
            string[] lineas = texto.Split('\n');
            bool encontrado = false;
            StringBuilder acknowledgement = new StringBuilder();

            foreach (var linea in lineas)
            {
                if (linea.ToLower().Contains("acknowledgement"))
                {
                    encontrado = true;
                    acknowledgement.AppendLine(linea);
                }
                else if (encontrado)
                {
                    if (string.IsNullOrWhiteSpace(linea)) break; // Detener en línea vacía
                    acknowledgement.AppendLine(linea);
                }
            }

            return acknowledgement.Length > 0 ? acknowledgement.ToString() : "No se encontró la sección Acknowledgement.";
        }

        public async Task<List<Funder>> FindAcknowledgementAsync(string texto)
        {
            var lineas = texto.Split('\n');
            var acknowledgement = new StringBuilder();
            bool enSeccion = false;

            var seccionesFinales = new Regex(@"^(declarations|funding|references|ethics|conflicts|author contributions|consent|data availability|appendix)", RegexOptions.IgnoreCase);
            var encabezado = new Regex(@"^acknowledg(e)?ments?:?", RegexOptions.IgnoreCase);

            foreach (var linea in lineas)
            {
                var trimmed = linea.Trim();

                // Si encontramos un encabezado de acknowledgements
                if (encabezado.IsMatch(trimmed))
                {
                    enSeccion = true;
                   
                    // Saltamos el título
                }

                // Si estamos dentro de una sección de acknowledgements
                if (enSeccion)
                {
                    // Si encontramos un nuevo encabezado/sección, cerramos la actual
                    if (seccionesFinales.IsMatch(trimmed))
                    {
                        enSeccion = false;
                        acknowledgement.AppendLine(); // Separar bloques por si hay más
                        continue;
                    }

                    // Agregar línea al texto
                    acknowledgement.AppendLine(trimmed);
                }
            }

            if (acknowledgement.Length > 0)
            {
                return await FindFundersWithLLMAsync(acknowledgement.ToString().Trim());
                //acknowledgement.ToString().Trim();
            }

            // Si no se encontró nada, usar el LLM
            return await FindFundersWithLLMAsync(texto);
        }


        private async Task<List<Funder>> FindFundersWithLLMAsync(string texto)
        {
            string systemPrompt = """
        Eres un asistente que extrae información de financiación de un artículo.
        Devuelve exclusivamente un JSON con esta forma:

        {
          "funders": [
            {
              "name": "Nombre del organismo",
              "identifier": "yyyyyy/xxxxxxxxxxxxx",
              "scheme": "fundref",
              "grant_number": "PIDyyyy-XXXXX"
            }
          ]
        }

        Si no hay funders, devuelve: { "funders": [] }
        No devuelvas ningún texto adicional, solo el JSON.
        """;

            string userPrompt = $"Texto fuente:\n\n{texto}";

            var payload = new
            {
                model = "mistral", // o el que uses en Ollama
                stream = false,
                messages = new[]
                {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userPrompt }
        }
            };

            var reqContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:11434/api/chat", reqContent);
            string raw = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(raw);
            JsonElement root = doc.RootElement;

            Console.WriteLine($"Respuesta del modelo: {raw}");
            // Comprueba si la ruta esperada existe
            if (!root.TryGetProperty("message", out var messageElement) ||
                !messageElement.TryGetProperty("content", out var contentElement))
            {
                Console.WriteLine("❌ No se encontró el contenido en la respuesta del modelo.");
                return new List<Funder>();
            }


            string contentJson = contentElement.GetString() ?? "";

            Console.WriteLine($"Contenido del modelo: {contentJson}");

            // ⚠️ Limpia si hay triple backticks
            if (contentJson.StartsWith("```"))
            {
                contentJson = contentJson.Trim('`').Trim();
                if (contentJson.StartsWith("json")) contentJson = contentJson[4..].Trim();
            }

            try
            {
                var funderObj = JsonSerializer.Deserialize<FunderWrapper>(contentJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });



                var firstValid = funderObj.Funders
                .FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.GrantNumber));

                return firstValid != null ? new List<Funder> { firstValid } : new List<Funder>();


               /* if (funderObj?.Funders is { Count: > 0 })
                {
                    var firstFunder = funderObj.Funders[0];

                    // Validar campos mínimos si es necesario
                    if (!string.IsNullOrWhiteSpace(firstFunder.Name) && !string.IsNullOrWhiteSpace(firstFunder.GrantNumber))
                    {
                        return new List<Funder> { firstFunder };
                    }
                }

                return new List<Funder>();*/

            }
            catch (JsonException ex)
            {
                Console.WriteLine($" Error parsing JSON del modelo: {ex.Message}");
                Console.WriteLine("Contenido recibido:\n" + contentJson);
                return new List<Funder>();
            }
        }

        // DTO envoltorio
        private record FunderWrapper(List<Funder> Funders);


        
        
    }
}
