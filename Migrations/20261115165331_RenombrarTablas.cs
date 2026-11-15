using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuSemanal.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarTablas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuDias_MenuSemanars_MenuSemanalId",
                table: "MenuDias");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuDias_Platoss_AlmuerzoId",
                table: "MenuDias");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuDias_Platoss_CenaId",
                table: "MenuDias");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuDias_Platoss_DesayunoId",
                table: "MenuDias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Platoss",
                table: "Platoss");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MenuSemanars",
                table: "MenuSemanars");

            migrationBuilder.RenameTable(
                name: "Platoss",
                newName: "Platos");

            migrationBuilder.RenameTable(
                name: "MenuSemanars",
                newName: "MenusSemanales");

            migrationBuilder.RenameIndex(
                name: "IX_Platoss_Nombre",
                table: "Platos",
                newName: "IX_Platos_Nombre");

            migrationBuilder.RenameIndex(
                name: "IX_MenuSemanars_SemanaFecha",
                table: "MenusSemanales",
                newName: "IX_MenusSemanales_SemanaFecha");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Platos",
                table: "Platos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MenusSemanales",
                table: "MenusSemanales",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuDias_MenusSemanales_MenuSemanalId",
                table: "MenuDias",
                column: "MenuSemanalId",
                principalTable: "MenusSemanales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuDias_Platos_AlmuerzoId",
                table: "MenuDias",
                column: "AlmuerzoId",
                principalTable: "Platos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuDias_Platos_CenaId",
                table: "MenuDias",
                column: "CenaId",
                principalTable: "Platos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuDias_Platos_DesayunoId",
                table: "MenuDias",
                column: "DesayunoId",
                principalTable: "Platos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuDias_MenusSemanales_MenuSemanalId",
                table: "MenuDias");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuDias_Platos_AlmuerzoId",
                table: "MenuDias");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuDias_Platos_CenaId",
                table: "MenuDias");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuDias_Platos_DesayunoId",
                table: "MenuDias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Platos",
                table: "Platos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MenusSemanales",
                table: "MenusSemanales");

            migrationBuilder.RenameTable(
                name: "Platos",
                newName: "Platoss");

            migrationBuilder.RenameTable(
                name: "MenusSemanales",
                newName: "MenuSemanars");

            migrationBuilder.RenameIndex(
                name: "IX_Platos_Nombre",
                table: "Platoss",
                newName: "IX_Platoss_Nombre");

            migrationBuilder.RenameIndex(
                name: "IX_MenusSemanales_SemanaFecha",
                table: "MenuSemanars",
                newName: "IX_MenuSemanars_SemanaFecha");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Platoss",
                table: "Platoss",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MenuSemanars",
                table: "MenuSemanars",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuDias_MenuSemanars_MenuSemanalId",
                table: "MenuDias",
                column: "MenuSemanalId",
                principalTable: "MenuSemanars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuDias_Platoss_AlmuerzoId",
                table: "MenuDias",
                column: "AlmuerzoId",
                principalTable: "Platoss",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuDias_Platoss_CenaId",
                table: "MenuDias",
                column: "CenaId",
                principalTable: "Platoss",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuDias_Platoss_DesayunoId",
                table: "MenuDias",
                column: "DesayunoId",
                principalTable: "Platoss",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
