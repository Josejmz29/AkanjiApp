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
            // Validar autores
            var validCreators = documento.Autores
                .Where(a => !string.IsNullOrWhiteSpace(a.Name) && !string.IsNullOrWhiteSpace(a.Apellido))
                .Select(a => new Dictionary<string, object>
                {
            { "name", $"{a.Name} {a.Apellido}" }
                    // Incluir ORCID si está disponible y es válido
                    // { "orcid", a.ORCID }
                })
                .ToArray();

            if (!validCreators.Any())
                throw new InvalidOperationException("Debe haber al menos un autor con nombre y apellido.");

            // Preparar campos opcionales con valores predeterminados
            var keywords = string.IsNullOrWhiteSpace(documento.Keywords)
                ? new[] { "sin-palabras-clave" }
                : documento.Keywords.Split(',').Select(k => k.Trim()).ToArray();

            

            var language = string.IsNullOrWhiteSpace(documento.Language) ? "en" : documento.Language;

            var publicationDate = documento.FechaPublicacion?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

            var description = string.IsNullOrWhiteSpace(documento.Description) ? "Sin descripción disponible." : documento.Description;

            var journalTitle = string.IsNullOrWhiteSpace(documento.Publisher) ? "Sin revista" : documento.Publisher;

            var subjects = documento.Subjects?
                .Where(s => !string.IsNullOrWhiteSpace(s.Text))
                .Select(s => new { subject = s.Text })
                .ToArray();

            var rightsList = documento.RightsList?
                .Where(r => !string.IsNullOrWhiteSpace(r.Rights))
                .Select(r => new
                {
                    rights = r.Rights,
                    rights_uri = r.RightsUri
                })
                .ToArray();

            /*var relatedIdentifiers = documento.RelatedIdentifiers?
                .Where(r => !string.IsNullOrWhiteSpace(r.Identifier) && !string.IsNullOrWhiteSpace(r.RelationType))
                .Select(r => new
                {
                    identifier = r.Identifier,
                    relation_type = r.RelationType,
                    resource_type = r.ResourceTypeGeneral
                })
                .ToArray();*/


           var relatedIdentifiers = documento.RelatedIdentifiers
            .Where(r => !string.IsNullOrWhiteSpace(r.Identifier) && !string.IsNullOrWhiteSpace(r.RelationType))
            .Select(r => new Dictionary<string, object>
            {
                { "identifier", r.Identifier },
                { "relation_type", r.RelationType },
                // Solo agregar si hay valor
                { "resource_type", r.ResourceTypeGeneral ?? null }
            }.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value))
            .ToList();

            var alternateIdentifiers = documento.AlternateIdentifiers?
                .Where(a => !string.IsNullOrWhiteSpace(a.Identifier))
                .Select(a => new
                {
                    alternate_identifier = a.Identifier,
                    alternate_identifier_type = a.Type ?? "doi"
                })
                .ToArray();

            var contributors = documento.Contributors?
                .Where(c => !string.IsNullOrWhiteSpace(c.Name) && !string.IsNullOrWhiteSpace(c.Apellido))
                .Select(c => new
                {
                    name = $"{c.Name} {c.Apellido}"
                    // Incluir roles si están disponibles
                    // role = c.Role
                })
                .ToArray();

            var version = string.IsNullOrWhiteSpace(documento.Version) ? "1.0" : documento.Version;

            // Construir el objeto de metadatos
            var metadata = new
            {
                metadata = new
                {
                    title = documento.Titulo ?? "Título no disponible",
                    upload_type = "publication",
                    publication_type = "article",
                    description,
                    creators = validCreators,
                    keywords,
                    language,
                    publication_date = publicationDate,
                    journal_title = journalTitle,
                    version,
                    contributors,
                    rights_list = rightsList,
                    related_identifiers = relatedIdentifiers,
                    alternate_identifiers = alternateIdentifiers,
                    subjects
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


            // Serializar y enviar a Zenodo
           /* var json = JsonSerializer.Serialize(metadata);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"https://zenodo.org/api/deposit/depositions/{depositoId}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var exception = new HttpRequestException($"Error al agregar metadatos: {response.StatusCode}");
                exception.Data["ZenodoResponse"] = responseContent;
                throw exception;
            }*/


        }

        // --- helpers -------------
        void AddIfHasContent<T>(IDictionary<string, object> dict, string key, IEnumerable<T>? value)
        {
            if (value is not null && value.Any()) dict[key] = value;
        }
        void AddIfNotNull(IDictionary<string, object> dict, string key, object? value)
        {
            if (value is not null) dict[key] = value;
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
