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

        public DbSet<Beneficiario> Beneficiarios { get; set; }
        public DbSet<SolicitudPrestamo> SolicitudesPrestamo { get; set; }
        public DbSet<DetalleSolicitud> DetallesSolicitud { get; set; }
        public DbSet<DetallePrestamo> DetallesPrestamo { get; set; }
        public DbSet<Entrega> Entregas { get; set; }
        public DbSet<ActaEntrega> ActasEntrega { get; set; }
        public DbSet<Devolucion> Devoluciones { get; set; }
        public DbSet<DetalleDevolucion> DetallesDevolucion { get; set; }
        public DbSet<EventoPrestamo> EventosPrestamo { get; set; }

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

            modelBuilder.Entity<Beneficiario>().HasOne(b=>b.Usuario).WithOne().HasForeignKey<Beneficiario>(b=>b.UsuarioId);
            modelBuilder.Entity<SolicitudPrestamo>().Property(s=>s.Estado).HasConversion<string>().HasMaxLength(25);
            modelBuilder.Entity<SolicitudPrestamo>().ToTable(t=>{
                t.HasCheckConstraint("CK_Solicitud_Periodo","[FechaFinPrevista] >= [FechaInicioPrevista]");
                t.HasCheckConstraint("CK_Solicitud_Estado","[Estado] IN ('Borrador','Pendiente','EnRevision','PendienteAjuste','Aprobada','Rechazada','Cancelada','Materializada')");
            });
            modelBuilder.Entity<SolicitudPrestamo>().HasIndex(s=>new {s.BeneficiarioId,s.FechaInicioPrevista,s.FechaFinPrevista,s.Estado});
            modelBuilder.Entity<DetalleSolicitud>().HasOne(d=>d.Solicitud).WithMany(s=>s.Detalles).HasForeignKey(d=>d.SolicitudPrestamoId);
            modelBuilder.Entity<DetalleSolicitud>().HasIndex(d=>new {d.SolicitudPrestamoId,d.CategoriaEquipoId}).IsUnique();
            modelBuilder.Entity<DetalleSolicitud>().ToTable(t=>t.HasCheckConstraint("CK_DetallesSolicitud_Cantidades", "[CantidadSolicitada] > 0 AND [CantidadReservada] >= 0 AND ([CantidadAprobada] IS NULL OR [CantidadAprobada] > 0) AND ([CantidadPropuesta] IS NULL OR [CantidadPropuesta] >= 0)"));
            modelBuilder.Entity<Prestamo>().HasOne(p=>p.Solicitud).WithOne(s=>s.Prestamo).HasForeignKey<Prestamo>(p=>p.SolicitudPrestamoId);
            modelBuilder.Entity<Prestamo>().Property(p=>p.Estado).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<Prestamo>().ToTable(t=>t.HasCheckConstraint("CK_Prestamo_Estado","[Estado] IN ('Activo','Vencido','Devuelto','Cerrado')"));
            modelBuilder.Entity<DetallePrestamo>().HasAlternateKey(d=>new {d.PrestamoId,d.EquipoId});
            modelBuilder.Entity<Entrega>().HasOne(e=>e.Prestamo).WithOne(p=>p.Entrega).HasForeignKey<Entrega>(e=>e.PrestamoId);
            modelBuilder.Entity<Entrega>().HasOne(e=>e.BeneficiarioReceptor).WithMany().HasForeignKey(e=>e.BeneficiarioReceptorId);
            modelBuilder.Entity<ActaEntrega>().HasOne(a=>a.Entrega).WithOne(e=>e.Acta).HasForeignKey<ActaEntrega>(a=>a.EntregaId);
            modelBuilder.Entity<Devolucion>().HasOne(d=>d.Prestamo).WithOne(p=>p.Devolucion).HasForeignKey<Devolucion>(d=>d.PrestamoId);
            modelBuilder.Entity<Devolucion>().HasAlternateKey(d=>new {d.Id,d.PrestamoId});
            modelBuilder.Entity<DetalleDevolucion>().HasOne(d=>d.Devolucion).WithMany(d=>d.Detalles).HasForeignKey(d=>new {d.DevolucionId,d.PrestamoId}).HasPrincipalKey(d=>new {d.Id,d.PrestamoId});
            modelBuilder.Entity<DetalleDevolucion>().HasOne(d=>d.DetallePrestamo).WithMany().HasForeignKey(d=>new {d.PrestamoId,d.EquipoId}).HasPrincipalKey(d=>new {d.PrestamoId,d.EquipoId});
            modelBuilder.Entity<DetalleDevolucion>().HasIndex(d=>new {d.DevolucionId,d.EquipoId}).IsUnique();

            // Sin borrado en cascada: la trazabilidad no se pierde y SQL Server
            // no admite varias rutas de cascada (origen/destino hacia la misma tabla)
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
