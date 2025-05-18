using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatioVision.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dispositivos",
                columns: table => new
                {
                    DispositivoIotId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Tipo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UltimaLocalizacao = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    UltimaAtualizacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispositivos", x => x.DispositivoIotId);
                });

            migrationBuilder.CreateTable(
                name: "Patios",
                columns: table => new
                {
                    PatioId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Nome = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Latitude = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    DispositivoIotId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patios", x => x.PatioId);
                    table.ForeignKey(
                        name: "FK_Patios_Dispositivos_DispositivoIotId",
                        column: x => x.DispositivoIotId,
                        principalTable: "Dispositivos",
                        principalColumn: "DispositivoIotId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Motos",
                columns: table => new
                {
                    MotoId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Modelo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Placa = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: true),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PatioId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DispositivoIotId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motos", x => x.MotoId);
                    table.ForeignKey(
                        name: "FK_Motos_Dispositivos_DispositivoIotId",
                        column: x => x.DispositivoIotId,
                        principalTable: "Dispositivos",
                        principalColumn: "DispositivoIotId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Motos_Patios_PatioId",
                        column: x => x.PatioId,
                        principalTable: "Patios",
                        principalColumn: "PatioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Motos_DispositivoIotId",
                table: "Motos",
                column: "DispositivoIotId");

            migrationBuilder.CreateIndex(
                name: "IX_Motos_PatioId",
                table: "Motos",
                column: "PatioId");

            migrationBuilder.CreateIndex(
                name: "IX_Patios_DispositivoIotId",
                table: "Patios",
                column: "DispositivoIotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Motos");

            migrationBuilder.DropTable(
                name: "Patios");

            migrationBuilder.DropTable(
                name: "Dispositivos");
        }
    }
}
