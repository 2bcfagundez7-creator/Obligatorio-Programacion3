using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obligatorio.LogicaAccesoDatos.Migrations
{
    /// <inheritdoc />
    public partial class AjustarAuditorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auditorias_Historias_HistoriaId",
                table: "Auditorias");

            migrationBuilder.AlterColumn<int>(
                name: "HistoriaId",
                table: "Auditorias",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Auditorias_Historias_HistoriaId",
                table: "Auditorias",
                column: "HistoriaId",
                principalTable: "Historias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auditorias_Historias_HistoriaId",
                table: "Auditorias");

            migrationBuilder.AlterColumn<int>(
                name: "HistoriaId",
                table: "Auditorias",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Auditorias_Historias_HistoriaId",
                table: "Auditorias",
                column: "HistoriaId",
                principalTable: "Historias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
