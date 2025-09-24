using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatioVision.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "PATIO",
                type: "DECIMAL(18, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "PATIO",
                type: "DECIMAL(18, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DtAtualizacao",
                table: "PATIO",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DtCadastro",
                table: "PATIO",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DtAtualizacao",
                table: "MOTO",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DtCadastro",
                table: "MOTO",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DtAtualizacao",
                table: "DISPOSITIVO_IOT",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DtCadastro",
                table: "DISPOSITIVO_IOT",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DtAtualizacao",
                table: "PATIO");

            migrationBuilder.DropColumn(
                name: "DtCadastro",
                table: "PATIO");

            migrationBuilder.DropColumn(
                name: "DtAtualizacao",
                table: "MOTO");

            migrationBuilder.DropColumn(
                name: "DtCadastro",
                table: "MOTO");

            migrationBuilder.DropColumn(
                name: "DtAtualizacao",
                table: "DISPOSITIVO_IOT");

            migrationBuilder.DropColumn(
                name: "DtCadastro",
                table: "DISPOSITIVO_IOT");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "PATIO",
                type: "DECIMAL(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "PATIO",
                type: "DECIMAL(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18, 2)");
        }
    }
}
