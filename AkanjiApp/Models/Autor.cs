using System.ComponentModel.DataAnnotations;

namespace AkanjiApp.Models
{
    public class Autor
    {
        [Key]
        public int Id { get; set; } // Clave primaria
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? ORCID { get; set; }
        public string? Afiliacion { get; set; }
        public string? Role { get; set; }

        public string? Tipo { get; set;}

        public virtual ICollection<DocumentoAutor> DocumentoAutores { get; set; } = new List<DocumentoAutor>();
    }
}
