using System.ComponentModel.DataAnnotations;

namespace AkanjiApp.Models.DTO
{
    public class DocumentoDTO
    {

        
            public string Titulo { get; set; }
            public string? Descripcion { get; set; }
            public string? Keywords { get; set; }
            public string? Language { get; set; }
            public string? FechaPublicacion { get; set; } // Se enviará como string "YYYY-MM-DD"
            public string? Publisher { get; set; }

            // Solo un autor por ahora
            public string? AutorNombre { get; set; }
            public string? AutorApellido { get; set; }
            public string? AutorORCID { get; set; }
        }


    }
