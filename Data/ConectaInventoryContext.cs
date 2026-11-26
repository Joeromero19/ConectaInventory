using Microsoft.EntityFrameworkCore;
using ConectaInventory.Models;

namespace ConectaInventory.Data
{
    public class ConectaInventoryContext : DbContext
    {
        public ConectaInventoryContext(DbContextOptions<ConectaInventoryContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<CategoriaEquipo> Categorias { get; set; }
        public DbSet<UbicacionBodega> Ubicaciones { get; set; }
        public DbSet<Donante> Donantes { get; set; }
        public DbSet<RecepcionDonacion> Recepciones { get; set; }
        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<CodigoQR> CodigosQR { get; set; }
        public DbSet<MovimientoInventario> Movimientos { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }
        public DbSet<DiagnosticoTecnico> Diagnosticos { get; set; }
        public DbSet<SolicitudBaja> SolicitudesBaja { get; set; }
        public DbSet<ActaBaja> Actas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Restricciones de unicidad del diccionario de datos (seccion 9.4.6)
            modelBuilder.Entity<Usuario>().HasIndex(u => u.NombreUsuario).IsUnique();
            modelBuilder.Entity<CategoriaEquipo>().HasIndex(c => c.Nombre).IsUnique();
            modelBuilder.Entity<UbicacionBodega>().HasIndex(u => u.Nombre).IsUnique();
            modelBuilder.Entity<Donante>().HasIndex(d => d.Nombre).IsUnique();
            modelBuilder.Entity<Equipo>().HasIndex(e => e.Serial).IsUnique();
            modelBuilder.Entity<CodigoQR>().HasIndex(q => q.Valor).IsUnique();
            modelBuilder.Entity<CodigoQR>().HasIndex(q => q.EquipoId).IsUnique();

            // Una recepcion duplicada = mismo donante y mismo lote
            modelBuilder.Entity<RecepcionDonacion>()
                .HasIndex(r => new { r.DonanteId, r.Lote }).IsUnique();

            // Relaciones uno a uno
            modelBuilder.Entity<CodigoQR>()
                .HasOne(q => q.Equipo).WithOne(e => e.CodigoQR)
                .HasForeignKey<CodigoQR>(q => q.EquipoId);

            modelBuilder.Entity<ActaBaja>()
                .HasOne(a => a.Solicitud).WithOne(s => s.Acta)
                .HasForeignKey<ActaBaja>(a => a.SolicitudBajaId);

            // Sin borrado en cascada: la trazabilidad no se pierde y SQL Server
            // no admite varias rutas de cascada (origen/destino hacia la misma tabla)
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
