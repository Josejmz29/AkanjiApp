using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AkanjiApp.Models
{
     public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Documento> Documentos { get; set; }
        public DbSet<Autor> Autores { get; set; }
        public DbSet<DocumentoAutor> DocumentoAutores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // IMPORTANTE: Llamar a base.OnModelCreating()

            modelBuilder.Entity<Usuario>().ToTable("Usuarios");

            modelBuilder.Entity<AlternateIdentifier>().HasNoKey();
            modelBuilder.Entity<LicenciaDerechos>().HasNoKey();
            modelBuilder.Entity<Subject>().HasNoKey();
            // Configuración de la relación muchos a muchos
            modelBuilder.Entity<DocumentoAutor>()
                .HasKey(da => new { da.DocumentoId, da.AutorId });


            modelBuilder.Entity<DocumentoAutor>()
                .HasOne(da => da.Documento)
                .WithMany(d => d.Autores)
                .HasForeignKey(da => da.DocumentoId);

            modelBuilder.Entity<DocumentoAutor>()
                .HasOne(da => da.Autor)
                .WithMany(a => a.DocumentoAutores)
                .HasForeignKey(da => da.AutorId);
        }
    }
    
    
}
