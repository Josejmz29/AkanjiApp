using AkanjiApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AkanjiApp.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll() => await _dbSet.ToListAsync();
        public async Task<T> GetById(int id) => await _dbSet.FindAsync(id);
        public async Task Add(T entity) { await _dbSet.AddAsync(entity); }
        public Task Update(T entity) { _dbSet.Update(entity); return Task.CompletedTask; }
        public async Task Delete(int id) { var entity = await _dbSet.FindAsync(id); if (entity != null) _dbSet.Remove(entity); }
        public async Task Save() => await _context.SaveChangesAsync();
    }
}
