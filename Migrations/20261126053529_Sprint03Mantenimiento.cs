using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConectaInventory.Migrations
{
    /// <inheritdoc />
    public partial class Sprint03Mantenimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertasPreventivas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipoId = table.Column<int>(type: "int", nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Atendida = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasPreventivas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertasPreventivas_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mantenimientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipoId = table.Column<int>(type: "int", nullable: false),
                    ReportadoPorId = table.Column<int>(type: "int", nullable: false),
                    TecnicoId = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DetalleDevolucionId = table.Column<int>(type: "int", nullable: true),
                    Falla = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Abierto = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mantenimientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mantenimientos_DetallesDevolucion_DetalleDevolucionId",
                        column: x => x.DetalleDevolucionId,
                        principalTable: "DetallesDevolucion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mantenimientos_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mantenimientos_Usuarios_ReportadoPorId",
                        column: x => x.ReportadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mantenimientos_Usuarios_TecnicoId",
                        column: x => x.TecnicoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilesTecnicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Especialidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Disponible = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilesTecnicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilesTecnicos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialesMantenimiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MantenimientoId = table.Column<int>(type: "int", nullable: false),
                    ResponsableId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialesMantenimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialesMantenimiento_Mantenimientos_MantenimientoId",
                        column: x => x.MantenimientoId,
                        principalTable: "Mantenimientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialesMantenimiento_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RepuestosMantenimiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MantenimientoId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepuestosMantenimiento", x => x.Id);
                    table.CheckConstraint("CK_Repuesto_CantidadCosto", "[Cantidad] > 0 AND [CostoUnitario] >= 0");
                    table.ForeignKey(
                        name: "FK_RepuestosMantenimiento_Mantenimientos_MantenimientoId",
                        column: x => x.MantenimientoId,
                        principalTable: "Mantenimientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertasPreventivas_EquipoId",
                table: "AlertasPreventivas",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesMantenimiento_MantenimientoId",
                table: "HistorialesMantenimiento",
                column: "MantenimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesMantenimiento_ResponsableId",
                table: "HistorialesMantenimiento",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Mantenimientos_DetalleDevolucionId",
                table: "Mantenimientos",
                column: "DetalleDevolucionId",
                unique: true,
                filter: "[DetalleDevolucionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Mantenimientos_EquipoId",
                table: "Mantenimientos",
                column: "EquipoId",
                unique: true,
                filter: "[Abierto] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Mantenimientos_ReportadoPorId",
                table: "Mantenimientos",
                column: "ReportadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Mantenimientos_TecnicoId",
                table: "Mantenimientos",
                column: "TecnicoId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesTecnicos_UsuarioId",
                table: "PerfilesTecnicos",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepuestosMantenimiento_MantenimientoId",
                table: "RepuestosMantenimiento",
                column: "MantenimientoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertasPreventivas");

            migrationBuilder.DropTable(
                name: "HistorialesMantenimiento");

            migrationBuilder.DropTable(
                name: "PerfilesTecnicos");

            migrationBuilder.DropTable(
                name: "RepuestosMantenimiento");

            migrationBuilder.DropTable(
                name: "Mantenimientos");
        }
    }
}
