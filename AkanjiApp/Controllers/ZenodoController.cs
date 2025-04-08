using AkanjiApp.Models;
using AkanjiApp.Models.DTO;
using AkanjiApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AkanjiApp.Controllers
{

    [ApiController]
    [Route("api/zenodo")]
    public class ZenodoController : ControllerBase
    {

        private readonly ZenodoService _zenodoService;
        private readonly PdfService _pdfService;

        public ZenodoController(ZenodoService zenodoService, PdfService pdfService)
        {
            _zenodoService = zenodoService;
            _pdfService = pdfService;
        }

        [HttpPost("subir")]
        public async Task<IActionResult> SubirDocumento([FromForm] IFormFile file, [FromForm] DocumentoDTO dto )
        {
            if (file == null || file.Length == 0)
                return BadRequest("Por favor, sube un archivo PDF.");

            if (dto == null)
                return BadRequest("Los metadatos del documento son obligatorios.");

            // Construir el objeto Documento con la información del DTO
            Documento documento = new Documento
            {
                DOI = "", // No tenemos DOI en este flujo
                Titulo = dto.Titulo,
               // description = dto.Descripcion,
               // keywords = dto.Keywords,
             //   language = dto.Language,
              //  FechaPublicacion = DateTime.TryParse(dto.FechaPublicacion, out var fecha) ? fecha : null,
              //  publisher = dto.Publisher,
                Autores = new List<DocumentoAutor>
        {
            new DocumentoAutor
            {
                Autor = new Autor
                {
                    Nombre = dto.AutorNombre,
                    Apellido = dto.AutorApellido,
                    ORCID = dto.AutorORCID
                }
            }
        }
            };

            string depositoId = await _zenodoService.CrearDepositoAsync();
            await _zenodoService.AgregarMetadatosAsync(depositoId, documento);
            await _zenodoService.SubirArchivoAsync(depositoId, file);
            await _zenodoService.PublicarDepositoAsync(depositoId);

            return Ok(new { message = "Documento subido y publicado en Zenodo.", depositoId });
        }

        [HttpPost("prueba")]
        public async Task<IActionResult> Prueba()
        {
          

            return Ok(new { message = "Prueba exitosa." });

           
        }
    }
}
