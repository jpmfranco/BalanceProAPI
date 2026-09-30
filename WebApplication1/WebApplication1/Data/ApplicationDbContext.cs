using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Gastos> Gastos { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<PerfilFinanciero> PerfilesFinancieros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usuario
            modelBuilder.Entity<Usuarios>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Correo).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Correo).IsUnique();
                entity.Property(e => e.Contrasena).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                // Sin HasDefaultValueSql: el valor lo asigna el código (DateTime.UtcNow)
                // para ser compatible con SQL Server y PostgreSQL.
            });

            // Gasto
            modelBuilder.Entity<Gastos>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Monto).HasColumnType("numeric(18,2)");
                entity.HasOne<Usuarios>()
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Ingreso
            modelBuilder.Entity<Ingreso>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Monto).HasColumnType("numeric(18,2)");
                entity.HasOne<Usuarios>()
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PerfilFinanciero
            modelBuilder.Entity<PerfilFinanciero>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<Usuarios>()
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}