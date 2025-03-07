using System.ComponentModel.DataAnnotations;

namespace AkanjiApp.Models
{
    public class Documento
    {

        /*Basic information*/
        [Key]
        public string DOI { get; set; }
        public string? Titulo { get; set; }
        public Autor? autor { get; set; }
        public virtual ICollection<DocumentoAutor> Autores { get; set; } = new List<DocumentoAutor>();
        public string? resourceType { get; set; }

        public DateTime? FechaPublicacion { get; set; }

        //Alternative title
        //creator empresa
        //license

        public string? description { get; set; }

        /*Recomended information*/
        public virtual IList<Autor>? contributors { get; set; } = new List<Autor>();
        public string? keywords { get; set; }
        public string? language { get; set; }
        //dates

        public string? version { get; set; }
        public string? publisher { get; set; }
        /*funding*/
        /*Alternate identifiers*/
        /*realted works*/
        /*references*/
        /*software*/
        /*publishsing info*/
        /*conference*/
        /*domain specific fields*/




    }
}
