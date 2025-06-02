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
        
        public DbSet<DocumentoAutor> DocumentoAutores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // IMPORTANTE: Llamar a base.OnModelCreating()

            modelBuilder.Entity<Usuario>().ToTable("Usuarios");


            modelBuilder.Entity<Documento>()
    .OwnsMany(d => d.RightsList, r =>
    {
        r.WithOwner().HasForeignKey("DocumentoDOI");
        r.Property<int>("Id");
        r.HasKey("Id");
    });


            modelBuilder.Entity<Documento>()
            .OwnsMany(d => d.Funders, f =>
            {
                f.WithOwner().HasForeignKey("DocumentoDOI");
                f.Property<int>("Id"); // EF requiere clave
                f.HasKey("Id");
            });


            modelBuilder.Entity<Documento>()
    .OwnsMany(d => d.AlternateIdentifiers, a =>
    {
        a.WithOwner().HasForeignKey("DocumentoDOI");
        a.Property<int>("Id");
        a.HasKey("Id");
    });

            modelBuilder.Entity<Documento>()
    .OwnsMany(d => d.Subjects, s =>
    {
        s.WithOwner().HasForeignKey("DocumentoDOI");
        s.Property<int>("Id");
        s.HasKey("Id");
    });

            modelBuilder.Entity<Documento>().OwnsMany(d => d.RelatedIdentifiers, r =>
            {
                r.WithOwner().HasForeignKey("DocumentoDOI");
                r.Property<int>("Id");
                r.HasKey("Id");
            });


            // Configuración de la relación muchos a muchos
            modelBuilder.Entity<DocumentoAutor>()
     .HasKey(da => da.Id);

            modelBuilder.Entity<DocumentoAutor>()
                .HasOne(da => da.Documento)
                .WithMany(d => d.Autores)
                .HasForeignKey(da => da.DocumentoId);
        }
    }
    
    
}
