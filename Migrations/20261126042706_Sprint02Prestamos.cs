using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConectaInventory.Migrations
{
    /// <inheritdoc />
    public partial class Sprint02Prestamos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prestamos_Equipos_EquipoId",
                table: "Prestamos");

            migrationBuilder.DropIndex(
                name: "IX_Prestamos_EquipoId",
                table: "Prestamos");

            migrationBuilder.RenameColumn(
                name: "FechaInicio",
                table: "Prestamos",
                newName: "FechaEntrega");

            migrationBuilder.RenameColumn(
                name: "EquipoId",
                table: "Prestamos",
                newName: "SolicitudPrestamoId");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Prestamos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BeneficiarioId",
                table: "Prestamos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaDevolucionPrevista",
                table: "Prestamos",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaProgramadaDevolucion",
                table: "Prestamos",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstitucionRepresentada",
                table: "Prestamos",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesFaltantes",
                table: "Prestamos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPrestamo",
                table: "Prestamos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Equipos",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "Beneficiarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TipoBeneficiario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficiarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Beneficiarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesPrestamo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrestamoId = table.Column<int>(type: "int", nullable: false),
                    EquipoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPrestamo", x => x.Id);
                    table.UniqueConstraint("AK_DetallesPrestamo_PrestamoId_EquipoId", x => new { x.PrestamoId, x.EquipoId });
                    table.ForeignKey(
                        name: "FK_DetallesPrestamo_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesPrestamo_Prestamos_PrestamoId",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Devoluciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrestamoId = table.Column<int>(type: "int", nullable: false),
                    ResponsableId = table.Column<int>(type: "int", nullable: false),
                    FechaRealRecepcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tardia = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devoluciones", x => x.Id);
                    table.UniqueConstraint("AK_Devoluciones_Id_PrestamoId", x => new { x.Id, x.PrestamoId });
                    table.ForeignKey(
                        name: "FK_Devoluciones_Prestamos_PrestamoId",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devoluciones_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Entregas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrestamoId = table.Column<int>(type: "int", nullable: false),
                    ResponsableId = table.Column<int>(type: "int", nullable: false),
                    BeneficiarioReceptorId = table.Column<int>(type: "int", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdentidadVerificada = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entregas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entregas_Beneficiarios_BeneficiarioReceptorId",
                        column: x => x.BeneficiarioReceptorId,
                        principalTable: "Beneficiarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entregas_Prestamos_PrestamoId",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entregas_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesPrestamo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeneficiarioId = table.Column<int>(type: "int", nullable: false),
                    FechaInicioPrevista = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFinPrevista = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    UsuarioDecisionId = table.Column<int>(type: "int", nullable: true),
                    MotivoDecision = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaDecision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Proposito = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    InstitucionRepresentada = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesPrestamo", x => x.Id);
                    table.CheckConstraint("CK_Solicitud_Estado", "[Estado] IN ('Borrador','Pendiente','EnRevision','PendienteAjuste','Aprobada','Rechazada','Cancelada','Materializada')");
                    table.CheckConstraint("CK_Solicitud_Periodo", "[FechaFinPrevista] >= [FechaInicioPrevista]");
                    table.ForeignKey(
                        name: "FK_SolicitudesPrestamo_Beneficiarios_BeneficiarioId",
                        column: x => x.BeneficiarioId,
                        principalTable: "Beneficiarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesPrestamo_Usuarios_UsuarioDecisionId",
                        column: x => x.UsuarioDecisionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesDevolucion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DevolucionId = table.Column<int>(type: "int", nullable: false),
                    PrestamoId = table.Column<int>(type: "int", nullable: false),
                    EquipoId = table.Column<int>(type: "int", nullable: false),
                    Condicion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Novedades = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InspeccionFavorable = table.Column<bool>(type: "bit", nullable: false),
                    DestinoEquipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesDevolucion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesDevolucion_DetallesPrestamo_PrestamoId_EquipoId",
                        columns: x => new { x.PrestamoId, x.EquipoId },
                        principalTable: "DetallesPrestamo",
                        principalColumns: new[] { "PrestamoId", "EquipoId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesDevolucion_Devoluciones_DevolucionId_PrestamoId",
                        columns: x => new { x.DevolucionId, x.PrestamoId },
                        principalTable: "Devoluciones",
                        principalColumns: new[] { "Id", "PrestamoId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesDevolucion_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActasEntrega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntregaId = table.Column<int>(type: "int", nullable: false),
                    FechaFirma = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoFirma = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ReferenciaDocumento = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NombreDocumento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HuellaSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DocumentoFirmado = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActasEntrega", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActasEntrega_Entregas_EntregaId",
                        column: x => x.EntregaId,
                        principalTable: "Entregas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesSolicitud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudPrestamoId = table.Column<int>(type: "int", nullable: false),
                    CategoriaEquipoId = table.Column<int>(type: "int", nullable: false),
                    CantidadSolicitada = table.Column<int>(type: "int", nullable: false),
                    CantidadAprobada = table.Column<int>(type: "int", nullable: true),
                    CantidadReservada = table.Column<int>(type: "int", nullable: false),
                    CantidadPropuesta = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesSolicitud", x => x.Id);
                    table.CheckConstraint("CK_DetallesSolicitud_Cantidades", "[CantidadSolicitada] > 0 AND [CantidadReservada] >= 0 AND ([CantidadAprobada] IS NULL OR [CantidadAprobada] > 0) AND ([CantidadPropuesta] IS NULL OR [CantidadPropuesta] >= 0)");
                    table.ForeignKey(
                        name: "FK_DetallesSolicitud_Categorias_CategoriaEquipoId",
                        column: x => x.CategoriaEquipoId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesSolicitud_SolicitudesPrestamo_SolicitudPrestamoId",
                        column: x => x.SolicitudPrestamoId,
                        principalTable: "SolicitudesPrestamo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventosPrestamo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudPrestamoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosPrestamo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosPrestamo_SolicitudesPrestamo_SolicitudPrestamoId",
                        column: x => x.SolicitudPrestamoId,
                        principalTable: "SolicitudesPrestamo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventosPrestamo_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prestamos_BeneficiarioId",
                table: "Prestamos",
                column: "BeneficiarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamos_SolicitudPrestamoId",
                table: "Prestamos",
                column: "SolicitudPrestamoId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Prestamo_Estado",
                table: "Prestamos",
                sql: "[Estado] IN ('Activo','Vencido','Devuelto','Cerrado')");

            migrationBuilder.CreateIndex(
                name: "IX_ActasEntrega_EntregaId",
                table: "ActasEntrega",
                column: "EntregaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiarios_UsuarioId",
                table: "Beneficiarios",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDevolucion_DevolucionId_EquipoId",
                table: "DetallesDevolucion",
                columns: new[] { "DevolucionId", "EquipoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDevolucion_DevolucionId_PrestamoId",
                table: "DetallesDevolucion",
                columns: new[] { "DevolucionId", "PrestamoId" });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDevolucion_EquipoId",
                table: "DetallesDevolucion",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDevolucion_PrestamoId_EquipoId",
                table: "DetallesDevolucion",
                columns: new[] { "PrestamoId", "EquipoId" });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPrestamo_EquipoId",
                table: "DetallesPrestamo",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesSolicitud_CategoriaEquipoId",
                table: "DetallesSolicitud",
                column: "CategoriaEquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesSolicitud_SolicitudPrestamoId_CategoriaEquipoId",
                table: "DetallesSolicitud",
                columns: new[] { "SolicitudPrestamoId", "CategoriaEquipoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devoluciones_PrestamoId",
                table: "Devoluciones",
                column: "PrestamoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devoluciones_ResponsableId",
                table: "Devoluciones",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_BeneficiarioReceptorId",
                table: "Entregas",
                column: "BeneficiarioReceptorId");

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_PrestamoId",
                table: "Entregas",
                column: "PrestamoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_ResponsableId",
                table: "Entregas",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosPrestamo_SolicitudPrestamoId",
                table: "EventosPrestamo",
                column: "SolicitudPrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosPrestamo_UsuarioId",
                table: "EventosPrestamo",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPrestamo_BeneficiarioId_FechaInicioPrevista_FechaFinPrevista_Estado",
                table: "SolicitudesPrestamo",
                columns: new[] { "BeneficiarioId", "FechaInicioPrevista", "FechaFinPrevista", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPrestamo_UsuarioDecisionId",
                table: "SolicitudesPrestamo",
                column: "UsuarioDecisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prestamos_Beneficiarios_BeneficiarioId",
                table: "Prestamos",
                column: "BeneficiarioId",
                principalTable: "Beneficiarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prestamos_SolicitudesPrestamo_SolicitudPrestamoId",
                table: "Prestamos",
                column: "SolicitudPrestamoId",
                principalTable: "SolicitudesPrestamo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prestamos_Beneficiarios_BeneficiarioId",
                table: "Prestamos");

            migrationBuilder.DropForeignKey(
                name: "FK_Prestamos_SolicitudesPrestamo_SolicitudPrestamoId",
                table: "Prestamos");

            migrationBuilder.DropTable(
                name: "ActasEntrega");

            migrationBuilder.DropTable(
                name: "DetallesDevolucion");

            migrationBuilder.DropTable(
                name: "DetallesSolicitud");

            migrationBuilder.DropTable(
                name: "EventosPrestamo");

            migrationBuilder.DropTable(
                name: "Entregas");

            migrationBuilder.DropTable(
                name: "DetallesPrestamo");

            migrationBuilder.DropTable(
                name: "Devoluciones");

            migrationBuilder.DropTable(
                name: "SolicitudesPrestamo");

            migrationBuilder.DropTable(
                name: "Beneficiarios");

            migrationBuilder.DropIndex(
                name: "IX_Prestamos_BeneficiarioId",
                table: "Prestamos");

            migrationBuilder.DropIndex(
                name: "IX_Prestamos_SolicitudPrestamoId",
                table: "Prestamos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Prestamo_Estado",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "BeneficiarioId",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "FechaDevolucionPrevista",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "FechaProgramadaDevolucion",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "InstitucionRepresentada",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "ObservacionesFaltantes",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "TipoPrestamo",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Equipos");

            migrationBuilder.RenameColumn(
                name: "SolicitudPrestamoId",
                table: "Prestamos",
                newName: "EquipoId");

            migrationBuilder.RenameColumn(
                name: "FechaEntrega",
                table: "Prestamos",
                newName: "FechaInicio");

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Prestamos",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Prestamos_EquipoId",
                table: "Prestamos",
                column: "EquipoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prestamos_Equipos_EquipoId",
                table: "Prestamos",
                column: "EquipoId",
                principalTable: "Equipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
