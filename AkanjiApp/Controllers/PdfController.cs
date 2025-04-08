using AkanjiApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace AkanjiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly PdfService _pdfService;

        public PdfController(PdfService pdfService)
        {
            _pdfService = pdfService;
        }
        [HttpPost("extraer-acknowledgement")]
        public async Task<IActionResult> ExtraerAcknowledgement([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Por favor, sube un archivo PDF.");

            string filePath = Path.GetTempFileName();

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string texto = _pdfService.ExtractText(filePath);
            string acknowledgement = await _pdfService.FindAcknowledgementAsync(texto); // Esperar el resultado asíncrono

            return Ok(new { acknowledgement });
        }

    }
}
