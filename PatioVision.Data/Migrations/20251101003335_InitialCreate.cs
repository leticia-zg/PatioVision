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
                name: "DISPOSITIVO_IOT",
                columns: table => new
                {
                    DispositivoIotId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Tipo = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    UltimaLocalizacao = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: true),
                    UltimaAtualizacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DtCadastro = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DtAtualizacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DISPOSITIVO_IOT", x => x.DispositivoIotId);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Nome = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Senha = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Perfil = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DtAlteracao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Ativo = table.Column<int>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PATIO",
                columns: table => new
                {
                    PatioId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Nome = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Latitude = table.Column<decimal>(type: "DECIMAL(18,10)", precision: 18, scale: 10, nullable: false),
                    Longitude = table.Column<decimal>(type: "DECIMAL(18,10)", precision: 18, scale: 10, nullable: false),
                    Capacidade = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DispositivoIotId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DtCadastro = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DtAtualizacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PATIO", x => x.PatioId);
                    table.ForeignKey(
                        name: "FK_PATIO_DISPOSITIVO_IOT_DispositivoIotId",
                        column: x => x.DispositivoIotId,
                        principalTable: "DISPOSITIVO_IOT",
                        principalColumn: "DispositivoIotId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MOTO",
                columns: table => new
                {
                    MotoId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Modelo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Placa = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PatioId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DispositivoIotId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DtCadastro = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DtAtualizacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOTO", x => x.MotoId);
                    table.ForeignKey(
                        name: "FK_MOTO_DISPOSITIVO_IOT_DispositivoIotId",
                        column: x => x.DispositivoIotId,
                        principalTable: "DISPOSITIVO_IOT",
                        principalColumn: "DispositivoIotId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MOTO_PATIO_PatioId",
                        column: x => x.PatioId,
                        principalTable: "PATIO",
                        principalColumn: "PatioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MOTO_DispositivoIotId",
                table: "MOTO",
                column: "DispositivoIotId");

            migrationBuilder.CreateIndex(
                name: "IX_MOTO_PatioId",
                table: "MOTO",
                column: "PatioId");

            migrationBuilder.CreateIndex(
                name: "IX_PATIO_DispositivoIotId",
                table: "PATIO",
                column: "DispositivoIotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MOTO");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "PATIO");

            migrationBuilder.DropTable(
                name: "DISPOSITIVO_IOT");
        }
    }
}
