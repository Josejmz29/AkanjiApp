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

            // ---------- 2.  Creadores ----------
            var creators = documento.Autores
                .Where(a => !string.IsNullOrWhiteSpace(a.Name) && !string.IsNullOrWhiteSpace(a.Apellido))
                .Select(a => new { name = $"{a.Name} {a.Apellido}" })
                .ToArray();

            if (!creators.Any())
                throw new InvalidOperationException("Debe haber al menos un autor con nombre y apellido.");

            // ---------- 3.  Diccionario base ----------

            var metadataDict = new Dictionary<string, object?>
            {
                ["title"] = documento.Titulo ?? "Título no disponible",
                ["upload_type"] = "publication",
                ["publication_type"] = "article",
                ["description"] = string.IsNullOrWhiteSpace(documento.Description) ? "Sin descripción disponible." : documento.Description,
                ["creators"] = creators,
                ["language"] = string.IsNullOrWhiteSpace(documento.Language) ? "en" : documento.Language,
                ["publication_date"] = (documento.FechaPublicacion ?? DateTime.UtcNow).ToString("yyyy-MM-dd"),
                ["version"] = string.IsNullOrWhiteSpace(documento.Version) ? "1.0" : documento.Version
            };

            

            // ---------- 4.  DOI (2 vías) ----------
            // A) Cabecera
            AddIfNotNull(metadataDict, "doi", documento.DOI);


            // ---------- 5.  Keywords, subjects, rights ----------

            if (!string.IsNullOrWhiteSpace(documento.Publisher))
                metadataDict["journal_title"] = documento.Publisher;



            var contributors = documento.Contributors?
                .Where(c => !string.IsNullOrWhiteSpace(c.Name) && !string.IsNullOrWhiteSpace(c.Apellido))
                .Select(c => new { name = $"{c.Name} {c.Apellido}" })
                .ToArray();

            if (contributors?.Any() == true)
                metadataDict["contributors"] = contributors;

            var rights = documento.RightsList?
                .Where(r => !string.IsNullOrWhiteSpace(r.Rights))
                .Select(r => new { rights = r.Rights, rights_uri = r.RightsUri })
                .ToArray();

            if (rights?.Any() == true)
                metadataDict["rights_list"] = rights;

            var related = documento.RelatedIdentifiers?
                .Where(r => !string.IsNullOrWhiteSpace(r.Identifier) && !string.IsNullOrWhiteSpace(r.RelationType))
                .Select(r =>
                {
                    var dict = new Dictionary<string, object>
                    {
                        ["identifier"] = r.Identifier!,
                        ["relation_type"] = r.RelationType!
                    };
                    if (!string.IsNullOrWhiteSpace(r.ResourceTypeGeneral))
                        dict["resource_type"] = r.ResourceTypeGeneral;
                    return dict;
                })
                .ToArray();

           /* if (related?.Any() == true)
                metadataDict["related_identifiers"] = related;*/

            var altIds = documento.AlternateIdentifiers?
                .Where(a => !string.IsNullOrWhiteSpace(a.Identifier))
                .Select(a => new
                {
                    alternate_identifier = a.Identifier,
                    alternate_identifier_type = string.IsNullOrWhiteSpace(a.Type) ? "doi" : a.Type
                })
                .ToArray();

           

            var subjects = documento.Subjects?
                .Where(s => !string.IsNullOrWhiteSpace(s.Text))
                .Select(s => new { subject = s.Text })
                .ToArray();

            if (subjects?.Any() == true)
            {
                metadataDict["subjects"] = subjects;

                // Extraer solo el texto plano para keywords
                var keywordTexts = subjects.Select(s => s.subject).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                if (keywordTexts.Any())
                    metadataDict["keywords"] = keywordTexts;

            }

            // ---------- 6.  Financiadores ----------
           var grants = documento.Funders?
            .Where(f => !string.IsNullOrWhiteSpace(f.Name) && !string.IsNullOrWhiteSpace(f.GrantNumber))
            .Select(f => new Dictionary<string, object>
            {
                ["funder"] = new Dictionary<string, object>
                {
                    ["name"] = f.Name,
                    ["identifier"] = f.Identifier,
                    ["scheme"] = f.Scheme ?? "fundref"
                },
                ["grant_number"] = f.GrantNumber
            })
            .ToList();

            if (grants?.Any() == true)
                metadataDict["grants"] = grants;

            // ---------- 6. Financiadores ----------
            /*if (documento.Funders != null && documento.Funders.Any())
            {
                var f = documento.Funders
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.GrantNumber))
                    .FirstOrDefault();

                if (f != null)
                {
                    var grant = new Dictionary<string, object>
                    {
                        ["funder"] = new Dictionary<string, object>
                        {
                            ["name"] = f.Name,
                            ["identifier"] = f.Identifier,
                            ["scheme"] = f.Scheme ?? "fundref"
                        },
                        ["grant_number"] = f.GrantNumber
                    };

                    metadataDict["grants"] = new[] { grant };
                }
            }*/

            // ---------- 7.  Enviar ----------
            var metadataPayload = new { metadata = metadataDict };

            var json = JsonSerializer.Serialize(metadataPayload);
            
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            using var content = new ByteArrayContent(jsonBytes);
            Console.WriteLine("Payload limpio:\n" + JsonSerializer.Serialize(metadataPayload, new JsonSerializerOptions { WriteIndented = true }));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PutAsync($"{_zenodoApiUrl}/{depositoId}", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"🔁 Respuesta de Zenodo (status {(int)response.StatusCode}):\n{responseBody}");

            

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Zenodo error: {response.StatusCode} - {errorContent}");
            }
            Console.WriteLine($"✅ Metadatos agregados correctamente al depósito {depositoId}");
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
