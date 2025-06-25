using AkanjiApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AkanjiApp.Services
{
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using AkanjiApp.Models;
    using System.Net.Http.Headers;
    using Microsoft.AspNetCore.Http;

    public class ZenodoV2Service
    {
        private readonly HttpClient _httpClient;
        private string _zenodoToken;
        private const string BaseUrl = "https://zenodo.org/api/records";

        public ZenodoV2Service(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _zenodoToken = string.Empty; // Inicializar el token como vacío
   
        }

        public Boolean SetZenodoToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("❌ Token de Zenodo no puede ser nulo o vacío.");
                return false;
            }

            _zenodoToken = token;
            Console.WriteLine("✅ Token de Zenodo configurado correctamente.");
            return true;
        }

        public async Task<string> CrearBorradorAsync()
        {
            PrepareAuthHeader();

            var json = "{}";
            // Convertimos el JSON a bytes y establecemos manualmente el Content-Type sin charset
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            using var content = new ByteArrayContent(jsonBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("id").ToString();
        }

        private void PrepareAuthHeader()
        {
            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _zenodoToken);
            }
        }

        public async Task AgregarMetadatosAsync(string recordId, Documento documento,string resourceType)
        {

            PrepareAuthHeader();
            // ---------- 1.  Creadores ----------

            var creators = documento.Autores
               .Where(a => !string.IsNullOrWhiteSpace(a.Name) && !string.IsNullOrWhiteSpace(a.Apellido))
               .Select(a => new
               {
                   person_or_org = new
                   {
                       type = "personal",
                       name = $"{a.Name} {a.Apellido}",
                       given_name = a.Name,
                       family_name = a.Apellido
                       // Puedes añadir ORCID u otros identificadores si están disponibles
                   }
               })
               .ToArray();

            if (!creators.Any())
                throw new InvalidOperationException("Debe haber al menos un autor con nombre y apellido.");
            
            // ---------- 2.  Diccionario base ----------

            var metadataDict = new Dictionary<string, object?>
            {
                ["title"] = documento.Titulo ?? "Título no disponible",

                ["description"] = string.IsNullOrWhiteSpace(documento.Description) ? "Sin descripción disponible." : documento.Description,
                ["creators"] = creators,

                ["publication_date"] = (documento.FechaPublicacion ?? DateTime.UtcNow).ToString("yyyy-MM-dd"),
                ["version"] = string.IsNullOrWhiteSpace(documento.Version) ? "1.0" : documento.Version,
                ["publisher"] = documento.Publisher ?? "Desconocido",
                ["access_right"] = "open", // Asumimos acceso abierto por defecto
                ["languages"] = new[] { string.IsNullOrWhiteSpace(documento.Language) ? "en" : documento.Language },
               
                // ["license"] = new Dictionary<string, string> { ["id"] = "cc-by-4.0" }, // Licencia CC BY 4.0 por defecto
            };

            string tipo = documento.ResourceType;
            if (resourceType.Equals("publication-article") || resourceType.Equals("publication-conferencepaper"))
            {
                tipo = resourceType;
            }
           

            metadataDict["resource_type"] = new Dictionary<string, string>
            {
                ["id"] = tipo
            };

            // ---------- 3.  Campos adicionales ----------
            AddIfNotNull(metadataDict, "doi", documento.DOI);
            AddIfNotNull(metadataDict, "journal_title", documento.Publisher);


            metadataDict["dates"] = new[] {
                new Dictionary<string,string> {
                    ["date"] = (documento.FechaPublicacion ?? DateTime.UtcNow).ToString("yyyy-MM-dd"),
                    ["type"] = "Issued"
                }
            };
            if (documento.Subjects?.Any() == true)
            {
                metadataDict["subjects"] = documento.Subjects.Select(s => new { subject = s.Text }).ToArray();
                metadataDict["keywords"] = documento.Subjects.Select(s => s.Text).ToArray();
            }

          

            if (documento.Funders?.Any() == true)
            {
                var fundingList = documento.Funders
                    .Where(f => !string.IsNullOrWhiteSpace(f.Name)) // Validar mínimo el nombre del financiador
                    .Select(f =>
                    {
                        var funder = new Dictionary<string, object>
                        {
                            ["name"] = f.Name
                        };

                        var fundingEntry = new Dictionary<string, object>
                        {
                            ["funder"] = funder
                        };

                        if (!string.IsNullOrWhiteSpace(f.GrantNumber))
                        {
                            fundingEntry["award"] = new Dictionary<string, object>
                            {
                                ["number"] = f.GrantNumber
                                // Puedes incluir "title" o "identifiers" si los tienes
                            };
                        }

                        return fundingEntry;
                    })
                    .ToList();

                metadataDict["funding"] = fundingList;
            }


            // ----- Armar payload completo con pids -----
            var payloadDict = new Dictionary<string, object>
            {
                ["metadata"] = metadataDict
            };

            if (!string.IsNullOrWhiteSpace(documento.DOI))
            {
                payloadDict["pids"] = new Dictionary<string, object>
                {
                    ["doi"] = new
                    {
                        identifier = documento.DOI,
                        provider = "external"
                    }
                };
            }

            var json = JsonSerializer.Serialize(payloadDict, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine("\n[Payload Final]:\n" + json);
            
           // Console.WriteLine("\n" + JsonSerializer.Serialize(payloadDict, new JsonSerializerOptions { WriteIndented = true }));
            
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            using var content = new ByteArrayContent(jsonBytes);
            

            var response = await _httpClient.PutAsync($"{BaseUrl}/{recordId}/draft", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Zenodo error: {response.StatusCode} - {responseBody}");

            Console.WriteLine($"✅ Metadatos agregados a {recordId}");
           
        }

        public async Task SubirArchivoAsync(string recordId, IFormFile file)
        {
            PrepareAuthHeader();


            // 1️⃣ Inicializar subida
            var initList = new[]
            {
                 new { key = file.FileName }
            };
            var initJson = JsonSerializer.Serialize(initList);
            using var initContent = new StringContent(initJson, Encoding.UTF8, "application/json");
            var initResp = await _httpClient.PostAsync($"{BaseUrl}/{recordId}/draft/files", initContent);
            var initBody = await initResp.Content.ReadAsStringAsync();
            if (!initResp.IsSuccessStatusCode)
                throw new HttpRequestException($"Error init upload: {initResp.StatusCode} - {initBody}");

            // 2️⃣ Enviar contenido
            using var steam = file.OpenReadStream();
            using var putContent = new StreamContent(steam);
            putContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var putResp = await _httpClient.PutAsync($"{BaseUrl}/{recordId}/draft/files/{file.FileName}/content", putContent);
            var putBody = await putResp.Content.ReadAsStringAsync();
            if (!putResp.IsSuccessStatusCode)
                throw new HttpRequestException($"Error uploading content: {putResp.StatusCode} - {putBody}");

            // 3️⃣ Commit
            var commitResp = await _httpClient.PostAsync($"{BaseUrl}/{recordId}/draft/files/{file.FileName}/commit", null);
            var commitBody = await commitResp.Content.ReadAsStringAsync();
            if (!commitResp.IsSuccessStatusCode)
                throw new HttpRequestException($"Error commits upload: {commitResp.StatusCode} - {commitBody}");

            Console.WriteLine($"✅ Archivo '{file.FileName}' subido correctamente.");

                 
           
        }

        public async Task PublicarAsync(string recordId)
        {
            PrepareAuthHeader();

            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/{recordId}/actions/publish", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"❌ Error al publicar: {response.StatusCode} - {responseBody}");

            Console.WriteLine("🚀 Depósito publicado exitosamente.");
        }

        // Helpers
        private void AddIfNotNull(IDictionary<string, object> dict, string key, object? value)
        {
            if (value is not null) dict[key] = value;
        }

       
    }
}
