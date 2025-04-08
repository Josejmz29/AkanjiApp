using System.Text.Json.Serialization;

namespace AkanjiApp.Models
{
    public class DocumentoAutor
    {
        public string DocumentoId { get; set; }
        public Documento Documento { get; set; }

        public int AutorId { get; set; }
        public Autor Autor { get; set; }




        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("affiliation")]
        public string? Affiliation { get; set; }
    }
}
