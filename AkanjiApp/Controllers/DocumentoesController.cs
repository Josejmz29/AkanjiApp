using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AkanjiApp.Models;
using AkanjiApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkanjiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly DoiService _doiService;

        public DocumentoesController(ApplicationDbContext context, DoiService doiService)
        {
            _context = context;
            _doiService = doiService;
        }

        // GET: api/Documentoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Documento>>> GetDocumentos()
        {
          if (_context.Documentos == null)
          {
              return NotFound();
          }
            return await _context.Documentos.ToListAsync();
        }

        // GET: api/Documentoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Documento>> GetDocumento(string id)
        {
          if (_context.Documentos == null)
          {
              return NotFound();
          }
            var documento = await _context.Documentos.FindAsync(id);

            if (documento == null)
            {
                return NotFound();
            }

            return documento;
        }

        // PUT: api/Documentoes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocumento(string id, Documento documento)
        {
            if (id != documento.DOI)
            {
                return BadRequest();
            }

            _context.Entry(documento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Documentoes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Documento>> PostDocumento(Documento documento)
        {
          if (_context.Documentos == null)
          {
              return Problem("Entity set 'ApplicationDbContext.Documentos'  is null.");
          }
            _context.Documentos.Add(documento);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DocumentoExists(documento.DOI))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetDocumento", new { id = documento.DOI }, documento);
        }

        // DELETE: api/Documentoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocumento(string id)
        {
            if (_context.Documentos == null)
            {
                return NotFound();
            }
            var documento = await _context.Documentos.FindAsync(id);
            if (documento == null)
            {
                return NotFound();
            }

            _context.Documentos.Remove(documento);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("/doi/{doi}")]
        public async Task<IActionResult> ObtenerDocumento(string doi)
        {
            var documento = await _doiService.ObtenerDocumentoPorDoiAsync(doi);
            if (documento == null)
            {
                return NotFound("Documento no encontrado");
            }
            return Ok(documento);
        }

        private bool DocumentoExists(string id)
        {
            return (_context.Documentos?.Any(e => e.DOI == id)).GetValueOrDefault();
        }
    }
}
