using Microsoft.EntityFrameworkCore;
using Obligatorio.LogicaNegocio.Entidades;

namespace Obligatorio.LogicaAccesoDatos.EF
{
    public class ObligatorioContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Lector> Lectores { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Historia> Historias { get; set; }
        public DbSet<Capitulo> Capitulos { get; set; }
        public DbSet<CapituloIntermedio> CapitulosIntermedios { get; set; }
        public DbSet<CapituloFinal> CapitulosFinales { get; set; }
        public DbSet<Opcion> Opciones { get; set; }
        public DbSet<Lectura> Lecturas { get; set; }

        public DbSet<Auditoria> Auditorias { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // En desarrollo usamos LocalDB. En SOMEE se configura ConnectionStrings__Obligatorio.
            string cadena = Environment.GetEnvironmentVariable("ConnectionStrings__Obligatorio")
                ?? @"Server=(localdb)\MSSQLLocalDB;Database=ObligatorioP3;Trusted_Connection=True;TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(cadena);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Auditoria>()
                .HasOne(a => a.Historia)
                .WithMany()
                .HasForeignKey(a => a.HistoriaId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}