using Microsoft.EntityFrameworkCore;
using PatioVision.Core.Models;

namespace PatioVision.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Moto> Motos { get; set; }
        public DbSet<Patio> Patios { get; set; }
        public DbSet<DispositivoIoT> Dispositivos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapear nomes das tabelas
            modelBuilder.Entity<Moto>().ToTable("MOTO");
            modelBuilder.Entity<Patio>().ToTable("PATIO");
            modelBuilder.Entity<DispositivoIoT>().ToTable("DISPOSITIVO_IOT");

            //Enum como string
            modelBuilder.Entity<Moto>()
                .Property(m => m.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Patio>()
                .Property(p => p.Categoria)
                .HasConversion<string>();

            modelBuilder.Entity<DispositivoIoT>()
                .Property(d => d.Tipo)
                .HasConversion<string>();

            // Relacionamentos
            modelBuilder.Entity<Moto>()
                .HasOne(m => m.Patio)
                .WithMany(p => p.Motos)
                .HasForeignKey(m => m.PatioId);

            modelBuilder.Entity<Moto>()
                .HasOne(m => m.Dispositivo)
                .WithMany()
                .HasForeignKey(m => m.DispositivoIotId);

            modelBuilder.Entity<Patio>()
                .HasOne(p => p.Dispositivo)
                .WithMany()
                .HasForeignKey(p => p.DispositivoIotId);
        }
    }
}
