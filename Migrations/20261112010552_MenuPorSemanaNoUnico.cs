using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuSemanal.Migrations
{
    /// <inheritdoc />
    public partial class MenuPorSemanaNoUnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MenuSemanars_SemanaFecha",
                table: "MenuSemanars");

            migrationBuilder.CreateIndex(
                name: "IX_MenuSemanars_SemanaFecha",
                table: "MenuSemanars",
                column: "SemanaFecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MenuSemanars_SemanaFecha",
                table: "MenuSemanars");

            migrationBuilder.CreateIndex(
                name: "IX_MenuSemanars_SemanaFecha",
                table: "MenuSemanars",
                column: "SemanaFecha",
                unique: true);
        }
    }
}
