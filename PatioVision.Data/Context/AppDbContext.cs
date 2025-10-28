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
        public DbSet<Usuario> Usuario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapear nomes das tabelas
            modelBuilder.Entity<Moto>().ToTable("MOTO");
            modelBuilder.Entity<Patio>().ToTable("PATIO");
            modelBuilder.Entity<DispositivoIoT>().ToTable("DISPOSITIVO_IOT");
            modelBuilder.Entity<Usuario>().ToTable("USUARIO");

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

            // Configurar precisão para Latitude e Longitude
            modelBuilder.Entity<Patio>()
                .Property(p => p.Latitude)
                .HasPrecision(18, 10);

            modelBuilder.Entity<Patio>()
                .Property(p => p.Longitude)
                .HasPrecision(18, 10);

            // Mapear BOOLEAN para NUMBER(1) no Oracle
            modelBuilder.Entity<Usuario>()
                .Property(u => u.Ativo)
                .HasColumnType("NUMBER(1)")
                .HasConversion<int>();
            
            // Configurar UltimaLocalizacao como nullable
            modelBuilder.Entity<DispositivoIoT>()
                .Property(d => d.UltimaLocalizacao)
                .IsRequired(false);

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
