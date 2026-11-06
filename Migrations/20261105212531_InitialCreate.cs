using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuSemanal.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuSemanars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SemanaFecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuSemanars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platoss",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Categoria = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platoss", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuDias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MenuSemanalId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiaSemana = table.Column<int>(type: "INTEGER", nullable: false),
                    DesayunoId = table.Column<int>(type: "INTEGER", nullable: true),
                    AlmuerzoId = table.Column<int>(type: "INTEGER", nullable: true),
                    CenaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuDias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuDias_MenuSemanars_MenuSemanalId",
                        column: x => x.MenuSemanalId,
                        principalTable: "MenuSemanars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuDias_Platoss_AlmuerzoId",
                        column: x => x.AlmuerzoId,
                        principalTable: "Platoss",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MenuDias_Platoss_CenaId",
                        column: x => x.CenaId,
                        principalTable: "Platoss",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MenuDias_Platoss_DesayunoId",
                        column: x => x.DesayunoId,
                        principalTable: "Platoss",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuDias_AlmuerzoId",
                table: "MenuDias",
                column: "AlmuerzoId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuDias_CenaId",
                table: "MenuDias",
                column: "CenaId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuDias_DesayunoId",
                table: "MenuDias",
                column: "DesayunoId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuDias_MenuSemanalId",
                table: "MenuDias",
                column: "MenuSemanalId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuSemanars_SemanaFecha",
                table: "MenuSemanars",
                column: "SemanaFecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platoss_Nombre",
                table: "Platoss",
                column: "Nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuDias");

            migrationBuilder.DropTable(
                name: "MenuSemanars");

            migrationBuilder.DropTable(
                name: "Platoss");
        }
    }
}
