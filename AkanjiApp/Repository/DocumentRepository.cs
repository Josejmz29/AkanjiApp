using AkanjiApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AkanjiApp.Repository
{
    public class DocumentRepository: Repository<Documento>, IDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

       

        public async Task<Documento> GetByDoiAsync(string doi)
        {
            return await _context.Documentos
                .Include(d => d.Autores)
                .Include(d => d.Contributors)
                .Include(d => d.RelatedIdentifiers)
                .Include(d => d.RightsList)
                .Include(d => d.Subjects)
                .Include(d => d.AlternateIdentifiers)
                .FirstOrDefaultAsync(d => d.DOI == doi);
        }
    }
}
