using System.Text;
using System.Text.Json;
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

        public async Task<string> FindAcknowledgementAsync(string texto)
        {
            string[] lineas = texto.Split('\n');
            bool encontrado = false;
            StringBuilder acknowledgement = new StringBuilder();

            foreach (var linea in lineas)
            {
                if (linea.ToLower().Contains("acknowledgement") )
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

            if (acknowledgement.Length > 0)
            {
                return acknowledgement.ToString();
            }

            // Si no se encuentra, usar el LLM
            return await FindAcknowledgementWithLLM(texto);
        }


        private async Task<string> FindAcknowledgementWithLLM(string texto)
        {
            var requestData = new
            {
                model = "mistral", // O el modelo que descargaste
                prompt = "Encuentra la sección de financiación o acknowledgement en este texto: " + texto,
                stream = false
            };

            var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:11434/api/generate", content);
            string result = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(result);
            JsonElement root = doc.RootElement;

            try
            {
                return root.GetProperty("response").GetString();
            }
            catch (KeyNotFoundException)
            {
                return "No se encontró la sección buscada por el modelo.";
            }

        }
    }
}
