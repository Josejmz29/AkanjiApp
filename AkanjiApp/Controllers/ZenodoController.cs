using AkanjiApp.Models;
using AkanjiApp.Models.DTO;
using AkanjiApp.Repository;
using AkanjiApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkanjiApp.Controllers
{

    [ApiController]
    [Route("api/zenodo")]
    public class ZenodoController : ControllerBase
    {

        private readonly ZenodoService _zenodoService;
        private readonly DocumentRepository _docRepository;
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

            Documento documento = dto.ToEntity();
            

            string depositoId = await _zenodoService.CrearDepositoAsync();
            await _zenodoService.AgregarMetadatosAsync(depositoId, documento);
            await _zenodoService.SubirArchivoAsync(depositoId, file);
            await _zenodoService.PublicarDepositoAsync(depositoId);

            return Ok(new { message = "Documento subido y publicado en Zenodo.", depositoId });
        }


        [HttpPost("subirDOi")]
        public async Task<IActionResult> SubirDocumento([FromForm] IFormFile file, [FromForm] string doi)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Por favor, sube un archivo PDF.");

            if (string.IsNullOrWhiteSpace(doi))
                return BadRequest("El DOI del documento es obligatorio.");

            // Buscar el documento en base de datos (ajusta si usas repositorio)
            var documento = await _docRepository
                .GetByDoiAsync(doi);

            

            if (documento == null)
                return NotFound($"No se encontró un documento con DOI: {doi}");

            // Crear depósito y subir a Zenodo
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
