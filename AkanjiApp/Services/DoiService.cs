using AkanjiApp.Models;
using System.Text.Json;
using System.Net.Http;

using System.Threading.Tasks;
using System.Text.RegularExpressions;

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

                Console.WriteLine($"❌ Error al acceder a CrossRef: {response.StatusCode}");
                return null; // O manejar error
            }

            string json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);


            var urlOA = $"https://api.openaire.eu/search/publications?doi={Uri.EscapeDataString(doi)}&format=json";

            var responseOA = await _httpClient.GetAsync(urlOA);

            if (!responseOA.IsSuccessStatusCode)
            {

                Console.WriteLine($"❌ Error al acceder a OpenAIRE: {responseOA.StatusCode}");
                return null; // O manejar error
            }

            var jsonOA = await responseOA.Content.ReadAsStringAsync();
            Console.WriteLine(jsonOA);


            var documento = ParsearJsonADocumento(json, jsonOA);
            return documento;
        }




        private Documento ParsearJsonADocumento(string json, string jsonOA)
        {
            Console.WriteLine(json);

            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement.GetProperty("message");

            using JsonDocument docOA = JsonDocument.Parse(jsonOA);
            var rootOA = docOA.RootElement;

            // Obtener fecha de publicación desde varias fuentes
            DateTime? fechaPublicacion = null;
            if (root.TryGetProperty("published-print", out var pubPrint) && pubPrint.TryGetProperty("date-parts", out var dateParts))
            {
                fechaPublicacion = new DateTime(dateParts[0][0].GetInt32(), 1, 1);
            }
            else if (root.TryGetProperty("published-online", out var pubOnline) && pubOnline.TryGetProperty("date-parts", out var datePartsOnline))
            {
                fechaPublicacion = new DateTime(datePartsOnline[0][0].GetInt32(), 1, 1);
            }
            else if (root.TryGetProperty("issued", out var issued) && issued.TryGetProperty("date-parts", out var datePartsIssued))
            {
                fechaPublicacion = new DateTime(datePartsIssued[0][0].GetInt32(), 1, 1);
            }

            // Autores
            List<DocumentoAutor> autores = new();
            if (root.TryGetProperty("author", out var authorArray) && authorArray.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement autor in authorArray.EnumerateArray())
                {
                    string nombre = autor.TryGetProperty("given", out var given) ? given.GetString() : null;
                    string apellido = autor.TryGetProperty("family", out var family) ? family.GetString() : null;
                    string orcid = autor.TryGetProperty("ORCID", out var orcidElement) ? orcidElement.GetString()?.Replace("https://orcid.org/", "") : null;
                    string afiliacion = autor.TryGetProperty("affiliation", out var affiliations) && affiliations.ValueKind == JsonValueKind.Array && affiliations.GetArrayLength() > 0
                        ? affiliations[0].GetProperty("name").GetString()
                        : null;

                    autores.Add(new DocumentoAutor
                    {
                        Name = $"{nombre}",
                        Apellido = apellido,
                        Affiliation = afiliacion
                    });
                }
            }

            // Contributors (si existen)
            List<DocumentoAutor> contributors = new();
            if (root.TryGetProperty("editor", out var editors) && editors.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement editor in editors.EnumerateArray())
                {
                    string nombre = editor.TryGetProperty("given", out var given) ? given.GetString() : null;
                    string apellido = editor.TryGetProperty("family", out var family) ? family.GetString() : null;
                    string afiliacion = editor.TryGetProperty("affiliation", out var affiliations) && affiliations.ValueKind == JsonValueKind.Array && affiliations.GetArrayLength() > 0
                        ? affiliations[0].GetProperty("name").GetString()
                        : null;

                    contributors.Add(new DocumentoAutor
                    {
                        Name = $"{nombre}",
                        Apellido = apellido,
                        Affiliation = afiliacion
                    });
                }
            }



            List<Subject> openAireSubjects = new();
            if (rootOA.TryGetProperty("response", out var response) &&
        response.TryGetProperty("results", out var results) &&
        results.TryGetProperty("result", out var resultArray) &&
        resultArray.ValueKind == JsonValueKind.Array && resultArray.GetArrayLength() > 0)
            {
                var firstResult = resultArray[0];

                if (firstResult.TryGetProperty("metadata", out var metadata) &&
                    metadata.TryGetProperty("oaf:entity", out var entity) &&
                    entity.TryGetProperty("oaf:result", out var oafResult) &&
                    oafResult.TryGetProperty("subject", out var subjectsEl) &&
                    subjectsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var subj in subjectsEl.EnumerateArray())
                    {
                        if (subj.TryGetProperty("$", out var textProp))
                        {
                            var text = textProp.GetString();
                            if (!string.IsNullOrWhiteSpace(text))
                                openAireSubjects.Add(new Subject { Text = text });
                        }
                    }
                }
            }





            // Rights
            List<LicenciaDerechos> rightsList = new();
            if (root.TryGetProperty("license", out var licenses) && licenses.ValueKind == JsonValueKind.Array)
            {
                foreach (var lic in licenses.EnumerateArray())
                {
                    rightsList.Add(new LicenciaDerechos
                    {
                        Rights = lic.GetProperty("URL").GetString(),
                        RightsUri = lic.GetProperty("URL").GetString()
                    });
                }
            }

            // Related identifiers
            List<RelatedIdentifier> related = new();
            if (root.TryGetProperty("reference", out var references) && references.ValueKind == JsonValueKind.Array)
            {
                foreach (var reference in references.EnumerateArray())
                {
                    if (reference.TryGetProperty("DOI", out var doiRef))
                    {
                        related.Add(new RelatedIdentifier
                        {
                            Identifier = doiRef.GetString(),
                            RelationType = "references"
                        });
                    }
                }
            }

            // Alternate identifiers
            List<AlternateIdentifier> alternateIdentifiers = new();
            if (root.TryGetProperty("alternative-id", out var altIds) && altIds.ValueKind == JsonValueKind.Array)
            {
                foreach (var alt in altIds.EnumerateArray())
                {
                    alternateIdentifiers.Add(new AlternateIdentifier
                    {
                        Identifier = alt.GetString(),
                        Type = "alternative-id"
                    });
                }
            }

            // Crear el objeto Documento
            var documento = new Documento
            {
                DOI = root.GetProperty("DOI").GetString(),
                Titulo = root.GetProperty("title")[0].GetString(),
                FechaPublicacion = fechaPublicacion,
                Description = root.TryGetProperty("abstract", out var abstractText)
                ? ExtraerDescripcionDesdeAbstract(abstractText.GetString()) : null,
                Publisher = root.GetProperty("publisher").GetString(),
                Language = root.TryGetProperty("language", out var lang) ? lang.GetString() : null,
                ResourceType = root.TryGetProperty("type", out var type) ? type.GetString() : null,
                Version = root.TryGetProperty("message-version", out var version) ? version.GetString() : null,
                Keywords = null,
                Autores = autores,
                Contributors = contributors,
                RightsList = rightsList,
                RelatedIdentifiers = related,
                Subjects = openAireSubjects,
                AlternateIdentifiers = alternateIdentifiers
            };



            return documento;
        }

        //++++++++++++++++++Auxiliares++++++++++++++++++++++++++++++++

        private string ExtraerDescripcionDesdeAbstract(string rawAbstract)
        {
            if (string.IsNullOrWhiteSpace(rawAbstract))
                return null;

            // Intenta extraer solo el primer párrafo del abstract
            var match = Regex.Match(rawAbstract, @"<jats:p>(.*?)<\/jats:p>", RegexOptions.Singleline);

            var contenido = match.Success ? match.Groups[1].Value : rawAbstract;

            // Elimina cualquier etiqueta <jats:*>
            contenido = Regex.Replace(contenido, @"<\/?jats:[^>]+>", "", RegexOptions.IgnoreCase);

            // Limpia otras etiquetas HTML/XML residuales si las hubiera
            contenido = Regex.Replace(contenido, @"<\/?[^>]+>", "", RegexOptions.IgnoreCase);

            return contenido.Trim();
        }




    }
}
