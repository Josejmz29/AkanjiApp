namespace AkanjiApp.Services
{
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using AkanjiApp.Models;
    using System.Net.Http.Headers;

    public class ZenodoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _zenodoApiUrl = "https://zenodo.org/api/deposit/depositions";
        private readonly string _zenodoToken;

        public ZenodoService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _zenodoToken = configuration["Zenodo:Token"]; // Almacena tu token en appsettings.json
        }

        /// <summary>
        /// Crea un nuevo depósito en Zenodo.
        /// </summary>
        public async Task<string> CrearDepositoAsync()
        {
            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_zenodoToken}");
            }

            // Cuerpo JSON mínimo
            var json = "{}";
            Console.WriteLine("JSON enviado a Zenodo: " + json);

            // Convertimos el JSON a bytes y establecemos manualmente el Content-Type sin charset
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            using var content = new ByteArrayContent(jsonBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            Console.WriteLine("Request Content-Type: " + content.Headers.ContentType);

            // Realizar la petición POST
            var response = await _httpClient.PostAsync(_zenodoApiUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Verificamos si fue exitosa
            response.EnsureSuccessStatusCode();

            using JsonDocument doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("id").ToString();
        }



        /// <summary>
        /// Añade metadatos al depósito.
        /// </summary>
        public async Task AgregarMetadatosAsync(string depositoId, Documento documento)
        {
            var validCreators = documento.Autores
        .Where(a => !string.IsNullOrWhiteSpace(a.Autor.Nombre) && !string.IsNullOrWhiteSpace(a.Autor.Apellido))
        .Select(a => {
            var creator = new Dictionary<string, object>
            {
                { "name", $"{a.Autor.Nombre} {a.Autor.Apellido}" }
            };
            // Solo incluir ORCID si es válido
           /* if (!string.IsNullOrWhiteSpace(a.Autor.ORCID) && IsValidOrcid(a.Autor.ORCID))
            {
                creator.Add("orcid", a.Autor.ORCID);
            }*/
            return creator;
        })
        .ToArray();

            if (validCreators.Length == 0)
            {
                Console.WriteLine("❌ No hay autores válidos.");
                throw new InvalidOperationException("Debe haber al menos un autor con nombre y apellido.");
            }

            var metadata = new
            {
                metadata = new
                {
                   /* title = documento.Titulo,
                    upload_type = "publication",
                    publication_type = "article",
                    description = documento.description,
                    creators = validCreators,
                    keywords = documento.keywords?.Split(',').Select(k => k.Trim()).ToArray(),
                    language = documento.language,
                    publication_date = documento.FechaPublicacion?.ToString("yyyy-MM-dd"),
                    journal_title = documento.publisher*/
                }
            };

            var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine("📦 Metadata JSON:\n" + json);

            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            using var content = new ByteArrayContent(jsonBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PutAsync($"{_zenodoApiUrl}/{depositoId}", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"🔁 Respuesta de Zenodo (status {(int)response.StatusCode}):\n{responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error {response.StatusCode}: {responseBody}");
            }

            Console.WriteLine($"✅ Metadatos agregados correctamente al depósito {depositoId}");
        }

        private bool IsValidOrcid(string orcid)
        {
            return !string.IsNullOrWhiteSpace(orcid) &&
                   System.Text.RegularExpressions.Regex.IsMatch(orcid, @"^(\d{4}-){3}\d{4}$");
        }





        /// <summary>
        /// Sube el archivo PDF a Zenodo.
        /// </summary>
        public async Task SubirArchivoAsync(string depositoId, IFormFile file)
        {
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            // (Opcional) Puedes establecer el Content-Type para el archivo si lo conoces, por ejemplo "application/pdf"
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            using var content = new MultipartFormDataContent();
            content.Add(streamContent, "file", file.FileName);

            var response = await _httpClient.PostAsync($"{_zenodoApiUrl}/{depositoId}/files", content);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Error al subir archivo: " + responseBody);
                throw new HttpRequestException($"Error {response.StatusCode}: {responseBody}");
            }
            else
            {
                Console.WriteLine("Archivo subido exitosamente.");
                Console.WriteLine("Response: " + await response.Content.ReadAsStringAsync());
            }
            response.EnsureSuccessStatusCode();
        }


        /// <summary>
        /// Publica el depósito en Zenodo.
        /// </summary>
        public async Task PublicarDepositoAsync(string depositoId)
        {
            var json = "{}";
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync($"{_zenodoApiUrl}/{depositoId}/actions/publish", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error al publicar: " + responseBody);
                throw new HttpRequestException($"Error {response.StatusCode}: {responseBody}");
            }
            else
            {

                Console.WriteLine("Depósito publicado exitosamente.");
                Console.WriteLine("Response: " + responseBody);
            }
        }


    }
}
