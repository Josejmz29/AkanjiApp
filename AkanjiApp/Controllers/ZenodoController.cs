using AkanjiApp.Models;
using AkanjiApp.Models.DTO;
using AkanjiApp.Repository;
using AkanjiApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Security.Claims;

namespace AkanjiApp.Controllers
{
    [Authorize]   
    [ApiController]
    [Route("api/zenodo")]
    public class ZenodoController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ZenodoService _zenodoService;
        private readonly IDocumentRepository _docRepository;
        private readonly PdfService _pdfService;
        private readonly ApplicationDbContext _context;
        private readonly ZenodoV2Service _zenodoV2Service;
        private readonly DoiService _doiService;

        public ZenodoController(ZenodoService zenodoService, PdfService pdfService, IDocumentRepository docRepository, 
                                ApplicationDbContext context, DoiService doiService, ZenodoV2Service zenodoV2Service,
                                UserManager<Usuario> userManager,                        // INYECTADO
                                IHttpClientFactory httpClientFactory)
        {
            _zenodoService = zenodoService;
            _pdfService = pdfService;
            _docRepository = docRepository;
            _context = context;
            _doiService = doiService;
            _zenodoV2Service = zenodoV2Service;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
        }

       

        [HttpPost("subirDOi-borradorV2")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubirBorradorV2(
    [FromForm] List<IFormFile> archivos,  // múltiples archivos
    [FromForm] string doi,
    [FromForm] string resourceType = "Default")
        {
            if (archivos == null || !archivos.Any())
                return BadRequest("Por favor, sube al menos un archivo.");

            if (string.IsNullOrWhiteSpace(doi))
                return BadRequest("El DOI del documento es obligatorio.");

            // ✅ Obtener el ID del usuario desde el token JWT
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _userManager.FindByIdAsync(userId);

            if (usuario == null || string.IsNullOrEmpty(usuario.ZenodoToken))
                return Unauthorized("Token de Zenodo no encontrado para el usuario.");

            
            

            try
            {

                _zenodoV2Service.SetZenodoToken(usuario.ZenodoToken);
                _zenodoService.SetZenodoToken(usuario.ZenodoToken);
                string decodedDoi = Uri.UnescapeDataString(doi);
                var documento = await _docRepository.GetByDoiAsync(decodedDoi);
                if (documento == null)
                    return NotFound($"No se encontró un documento con DOI: {decodedDoi}");

                string depositoId = await _zenodoV2Service.CrearBorradorAsync();
                await _zenodoV2Service.AgregarMetadatosAsync(depositoId, documento, resourceType);

                // Llama al nuevo método que maneja múltiples archivos
                await _zenodoService.SubirArchivosAsync(depositoId, archivos);

                return Ok(new { message = "Archivos subidos exitosamente a Zenodo.", depositoId });
            }
            catch (HttpRequestException httpEx)
            {
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



        [HttpPost("subirDOi-pubV2")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubirPublicacionV2(
    [FromForm] List<IFormFile> archivos,  // múltiples archivos
    [FromForm] string doi,
    [FromForm] string resourceType = "Default")
        {
            if (archivos == null || !archivos.Any())
                return BadRequest("Por favor, sube al menos un archivo.");

            if (string.IsNullOrWhiteSpace(doi))
                return BadRequest("El DOI del documento es obligatorio.");

            // ✅ Obtener el ID del usuario desde el token JWT
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _userManager.FindByIdAsync(userId);

            if (usuario == null || string.IsNullOrEmpty(usuario.ZenodoToken))
                return Unauthorized("Token de Zenodo no encontrado para el usuario.");




            try
            {

                _zenodoV2Service.SetZenodoToken(usuario.ZenodoToken);
                _zenodoService.SetZenodoToken(usuario.ZenodoToken);
                string decodedDoi = Uri.UnescapeDataString(doi);
                var documento = await _docRepository.GetByDoiAsync(decodedDoi);
                if (documento == null)
                    return NotFound($"No se encontró un documento con DOI: {decodedDoi}");

                string depositoId = await _zenodoV2Service.CrearBorradorAsync();
                await _zenodoV2Service.AgregarMetadatosAsync(depositoId, documento, resourceType);

                // Llama al nuevo método que maneja múltiples archivos
                await _zenodoService.SubirArchivosAsync(depositoId, archivos);
                await _zenodoV2Service.PublicarBorradorAsync(depositoId);

                return Ok(new { message = "Depósito publicado exitosamente a Zenodo.", depositoId });
            }
            catch (HttpRequestException httpEx)
            {
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


       
    }
}
