using AkanjiApp.Models;

namespace AkanjiApp.Repository
{
    public class AutorRepository: Repository<Autor>, IAutorRepository
    {
        private readonly ApplicationDbContext _context;

        public AutorRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
    
    
}
