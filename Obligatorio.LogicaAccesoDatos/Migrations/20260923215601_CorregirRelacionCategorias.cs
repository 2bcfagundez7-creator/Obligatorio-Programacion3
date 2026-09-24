using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obligatorio.LogicaAccesoDatos.Migrations
{
    /// <inheritdoc />
    public partial class CorregirRelacionCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Historias_HistoriaId",
                table: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_HistoriaId",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "HistoriaId",
                table: "Categorias");

            migrationBuilder.CreateTable(
                name: "CategoriaHistoria",
                columns: table => new
                {
                    CategoriasId = table.Column<int>(type: "int", nullable: false),
                    HistoriasId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaHistoria", x => new { x.CategoriasId, x.HistoriasId });
                    table.ForeignKey(
                        name: "FK_CategoriaHistoria_Categorias_CategoriasId",
                        column: x => x.CategoriasId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoriaHistoria_Historias_HistoriasId",
                        column: x => x.HistoriasId,
                        principalTable: "Historias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaHistoria_HistoriasId",
                table: "CategoriaHistoria",
                column: "HistoriasId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoriaHistoria");

            migrationBuilder.AddColumn<int>(
                name: "HistoriaId",
                table: "Categorias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_HistoriaId",
                table: "Categorias",
                column: "HistoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Historias_HistoriaId",
                table: "Categorias",
                column: "HistoriaId",
                principalTable: "Historias",
                principalColumn: "Id");
        }
    }
}
