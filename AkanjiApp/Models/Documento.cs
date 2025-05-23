using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AkanjiApp.Models
{
    

        public class Documento
        {
            /* Basic information */
            [Key]
            [JsonPropertyName("doi")]
            public string DOI { get; set; }

            [JsonPropertyName("title")]
            public string? Titulo { get; set; }

            [JsonPropertyName("creators")]
            public virtual ICollection<DocumentoAutor> Autores { get; set; } = new List<DocumentoAutor>();

            [JsonPropertyName("publication_date")]
            public DateTime? FechaPublicacion { get; set; }

            [JsonPropertyName("resource_type_general")]
            public string? ResourceType { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            /* Recommended information */
            [JsonPropertyName("contributors")]
            public virtual IList<DocumentoAutor>? Contributors { get; set; } = new List<DocumentoAutor>();

            [JsonPropertyName("keywords")]
            public string? Keywords { get; set; }

            [JsonPropertyName("language")]
            public string? Language { get; set; }

            [JsonPropertyName("version")]
            public string? Version { get; set; }

            [JsonPropertyName("publisher")]
            public string? Publisher { get; set; }

            [JsonPropertyName("related_identifiers")]
            public List<RelatedIdentifier>? RelatedIdentifiers { get; set; }

            [JsonPropertyName("rights_list")]
            public List<LicenciaDerechos>? RightsList { get; set; }

            [JsonPropertyName("subjects")]
            public List<Subject>? Subjects { get; set; }

            [JsonPropertyName("alternate_identifiers")]
            public List<AlternateIdentifier>? AlternateIdentifiers { get; set; }

        public void ActualizarDesde(Documento nuevo)
        {
            // Si quieres reemplazar relaciones también:
           

            Autores = nuevo.Autores;
            Contributors = nuevo.Contributors;
            RelatedIdentifiers = nuevo.RelatedIdentifiers;

            RightsList = nuevo.RightsList;
            Subjects = nuevo.Subjects;
            AlternateIdentifiers = nuevo.AlternateIdentifiers;

        }

    }





    public class RelatedIdentifier
    {

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("relation_type")]
        public string RelationType { get; set; }

        [JsonPropertyName("resource_type_general")]
        public string? ResourceTypeGeneral { get; set; }

       // public string? DoiAssertedBy { get; set; }

       // public string? Autor { get; set; }

      //  public string? Title { get; set; }
    }

    [Owned]
    public class LicenciaDerechos
        {
            [JsonPropertyName("rights")]
            public string? Rights { get; set; }

            [JsonPropertyName("rightsUri")]
            public string? RightsUri { get; set; }
        }


    [Owned]
        public class Subject
        {
            [JsonPropertyName("subject")]
            public string? Text { get; set; }
        }

            [Owned]
    public class AlternateIdentifier
        {
            
            [JsonPropertyName("alternate_identifier")]
            public string Identifier { get; set; }

            [JsonPropertyName("alternate_identifier_type")]
            public string? Type { get; set; }
        }
    }
