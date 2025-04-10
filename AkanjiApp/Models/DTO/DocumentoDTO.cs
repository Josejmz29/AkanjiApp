using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AkanjiApp.Models.DTO
{
    public class DocumentoDTO
    {

        [JsonPropertyName("doi")]
        public string DOI { get; set; }

        [JsonPropertyName("title")]
        public string? Titulo { get; set; }

        [JsonPropertyName("creators")]
        public List<DocumentoAutorDTO> Autores { get; set; } = new();

        [JsonPropertyName("publication_date")]
        public DateTime? FechaPublicacion { get; set; }

        [JsonPropertyName("resource_type_general")]
        public string? ResourceType { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("contributors")]
        public List<DocumentoAutorDTO>? Contributors { get; set; } = new();

        [JsonPropertyName("keywords")]
        public string? Keywords { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("publisher")]
        public string? Publisher { get; set; }

        [JsonPropertyName("related_identifiers")]
        public List<RelatedIdentifierDTO>? RelatedIdentifiers { get; set; }

        [JsonPropertyName("rights_list")]
        public List<LicenciaDerechosDTO>? RightsList { get; set; }

        [JsonPropertyName("subjects")]
        public List<SubjectDTO>? Subjects { get; set; }

        [JsonPropertyName("alternate_identifiers")]
        public List<AlternateIdentifierDTO>? AlternateIdentifiers { get; set; }


    }

    public class DocumentoAutorDTO
    {
        public string Name { get; set; }
        public string? Affiliation { get; set; }
        public string? ORCID { get; set; }
        public string? Role { get; set; }
        public string? Tipo { get; set; }
    }

    public class AutorDTO
    {
        public string Name { get; set; }
        public string? Affiliation { get; set; }
    }

    public class RelatedIdentifierDTO
    {
        public string Identifier { get; set; }
        public string RelationType { get; set; }
        public string? ResourceTypeGeneral { get; set; }
    }

    public class LicenciaDerechosDTO
    {
        public string Rights { get; set; }
        public string? RightsUri { get; set; }
    }

    public class SubjectDTO
    {
        public string Text { get; set; }
    }

    public class AlternateIdentifierDTO
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
    }


}
