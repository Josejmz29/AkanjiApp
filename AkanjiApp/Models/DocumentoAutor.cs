using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AkanjiApp.Models
{
    public class DocumentoAutor
    {
      
        [JsonPropertyName("name")]
        public string? Name { get; set; }
       
        public string? Apellido { get; set; }

        [JsonPropertyName("affiliation")]
        public string? Affiliation { get; set; }

        [Key]
        public int Id { get; set; }

        public string? DocumentoId { get; set; }
        public string? ORCID { get; set; }
        public string? Role { get; set; }
        public string? Tipo { get; set; }

        [JsonIgnore]
        public virtual Documento? Documento { get; set; }
    }
}
