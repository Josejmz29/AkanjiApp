using AkanjiApp.Models;
using System.Text.Json;
using System.Net.Http;

using System.Threading.Tasks;


namespace AkanjiApp.Services
{
    public class DoiService
    {
        private readonly HttpClient _httpClient;

        public DoiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Documento?> ObtenerDocumentoPorDoiAsync(string doi)
        {
            string url = $"https://api.crossref.org/works/{doi}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return null; // O manejar error
            }
            /*
             10.1103/PhysRevLett.116.061102
             */
            string json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);
            var documento = ParsearJsonADocumento(json);
            return documento;
        }

        private Documento ParsearJsonADocumento(string json)
        {
            Console.WriteLine(json);    
            
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement.GetProperty("message");

            return new Documento
            {
                DOI = root.GetProperty("DOI").GetString(),
                Titulo = root.GetProperty("title")[0].GetString(),
                FechaPublicacion = root.TryGetProperty("published-print", out var pubPrint)
                    ? new DateTime(pubPrint.GetProperty("date-parts")[0][0].GetInt32(), 1, 1)
                    : (DateTime?)null,
                description = root.TryGetProperty("abstract", out var abstractText) ? abstractText.GetString() : null,
                publisher = root.GetProperty("publisher").GetString(),
                language = root.TryGetProperty("language", out var lang) ? lang.GetString() : null
            };
        }
    }
}
