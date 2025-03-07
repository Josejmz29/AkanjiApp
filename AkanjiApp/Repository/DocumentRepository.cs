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

        public async Task<IEnumerable<Documento>> GetByTituloAsync(string titulo)
        {
            return await _context.Documentos
                .Where(d => d.Titulo.Contains(titulo))
                .ToListAsync();
        }
    }
}
