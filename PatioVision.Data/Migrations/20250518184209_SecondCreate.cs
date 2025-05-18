using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatioVision.Data.Migrations
{
    /// <inheritdoc />
    public partial class SecondCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Motos_Dispositivos_DispositivoIotId",
                table: "Motos");

            migrationBuilder.DropForeignKey(
                name: "FK_Motos_Patios_PatioId",
                table: "Motos");

            migrationBuilder.DropForeignKey(
                name: "FK_Patios_Dispositivos_DispositivoIotId",
                table: "Patios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Patios",
                table: "Patios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Motos",
                table: "Motos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Dispositivos",
                table: "Dispositivos");

            migrationBuilder.RenameTable(
                name: "Patios",
                newName: "PATIO");

            migrationBuilder.RenameTable(
                name: "Motos",
                newName: "MOTO");

            migrationBuilder.RenameTable(
                name: "Dispositivos",
                newName: "DISPOSITIVO_IOT");

            migrationBuilder.RenameIndex(
                name: "IX_Patios_DispositivoIotId",
                table: "PATIO",
                newName: "IX_PATIO_DispositivoIotId");

            migrationBuilder.RenameIndex(
                name: "IX_Motos_PatioId",
                table: "MOTO",
                newName: "IX_MOTO_PatioId");

            migrationBuilder.RenameIndex(
                name: "IX_Motos_DispositivoIotId",
                table: "MOTO",
                newName: "IX_MOTO_DispositivoIotId");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_PATIO",
                table: "PATIO",
                column: "PatioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MOTO",
                table: "MOTO",
                column: "MotoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DISPOSITIVO_IOT",
                table: "DISPOSITIVO_IOT",
                column: "DispositivoIotId");

            migrationBuilder.AddForeignKey(
                name: "FK_MOTO_DISPOSITIVO_IOT_DispositivoIotId",
                table: "MOTO",
                column: "DispositivoIotId",
                principalTable: "DISPOSITIVO_IOT",
                principalColumn: "DispositivoIotId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MOTO_PATIO_PatioId",
                table: "MOTO",
                column: "PatioId",
                principalTable: "PATIO",
                principalColumn: "PatioId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PATIO_DISPOSITIVO_IOT_DispositivoIotId",
                table: "PATIO",
                column: "DispositivoIotId",
                principalTable: "DISPOSITIVO_IOT",
                principalColumn: "DispositivoIotId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MOTO_DISPOSITIVO_IOT_DispositivoIotId",
                table: "MOTO");

            migrationBuilder.DropForeignKey(
                name: "FK_MOTO_PATIO_PatioId",
                table: "MOTO");

            migrationBuilder.DropForeignKey(
                name: "FK_PATIO_DISPOSITIVO_IOT_DispositivoIotId",
                table: "PATIO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PATIO",
                table: "PATIO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MOTO",
                table: "MOTO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DISPOSITIVO_IOT",
                table: "DISPOSITIVO_IOT");

            migrationBuilder.RenameTable(
                name: "PATIO",
                newName: "Patios");

            migrationBuilder.RenameTable(
                name: "MOTO",
                newName: "Motos");

            migrationBuilder.RenameTable(
                name: "DISPOSITIVO_IOT",
                newName: "Dispositivos");

            migrationBuilder.RenameIndex(
                name: "IX_PATIO_DispositivoIotId",
                table: "Patios",
                newName: "IX_Patios_DispositivoIotId");

            migrationBuilder.RenameIndex(
                name: "IX_MOTO_PatioId",
                table: "Motos",
                newName: "IX_Motos_PatioId");

            migrationBuilder.RenameIndex(
                name: "IX_MOTO_DispositivoIotId",
                table: "Motos",
                newName: "IX_Motos_DispositivoIotId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Patios",
                type: "DECIMAL(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Patios",
                type: "DECIMAL(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18, 2)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patios",
                table: "Patios",
                column: "PatioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Motos",
                table: "Motos",
                column: "MotoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dispositivos",
                table: "Dispositivos",
                column: "DispositivoIotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Motos_Dispositivos_DispositivoIotId",
                table: "Motos",
                column: "DispositivoIotId",
                principalTable: "Dispositivos",
                principalColumn: "DispositivoIotId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Motos_Patios_PatioId",
                table: "Motos",
                column: "PatioId",
                principalTable: "Patios",
                principalColumn: "PatioId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Patios_Dispositivos_DispositivoIotId",
                table: "Patios",
                column: "DispositivoIotId",
                principalTable: "Dispositivos",
                principalColumn: "DispositivoIotId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
