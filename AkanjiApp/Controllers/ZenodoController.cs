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
        private readonly IDocumentRepository _docRepository;
        private readonly PdfService _pdfService;

        public ZenodoController(ZenodoService zenodoService, PdfService pdfService, IDocumentRepository docRepository)
        {
            _zenodoService = zenodoService;
            _pdfService = pdfService;
            _docRepository = docRepository;
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubirDocumento([FromForm] IFormFile file, [FromForm] string doi)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Por favor, sube un archivo PDF.");

            if (string.IsNullOrWhiteSpace(doi))
                return BadRequest("El DOI del documento es obligatorio.");

            try
            {
                string decodedDoi = Uri.UnescapeDataString(doi);
                var documento = await _docRepository.GetByDoiAsync(decodedDoi);

                if (documento == null)
                    return NotFound($"No se encontró un documento con DOI: {decodedDoi}");

                string depositoId = await _zenodoService.CrearDepositoAsync();
                await _zenodoService.AgregarMetadatosAsync(depositoId, documento);
                await _zenodoService.SubirArchivoAsync(depositoId, file);
                await _zenodoService.PublicarDepositoAsync(depositoId);

                return Ok(new { message = "Documento subido y publicado en Zenodo.", depositoId });
            }
            catch (HttpRequestException httpEx)
            {
                // Intenta extraer respuesta de error detallada si está disponible
                if (httpEx.Data.Contains("ZenodoResponse"))
                {
                    var zenodoError = httpEx.Data["ZenodoResponse"]?.ToString();
                    return StatusCode(400, $"❌ Error de la API de Zenodo: {zenodoError}");
                }

                return StatusCode(500, $"❌ Error al comunicarse con Zenodo: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Error interno: {ex.Message}");
            }
        }


        [HttpPost("prueba")]
        public async Task<IActionResult> Prueba()
        {
          

            return Ok(new { message = "Prueba exitosa." });

           
        }
    }
}
