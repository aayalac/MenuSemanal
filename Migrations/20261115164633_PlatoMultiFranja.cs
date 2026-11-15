using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuSemanal.Migrations
{
    /// <summary>
    /// Issue #5: un plato puede pertenecer a varias franjas.
    /// - Categoria (0/1/2) pasa a Franjas, un enum [Flags] (1/2/4, combinables).
    /// - Los platos con el mismo nombre en distintas franjas se fusionan en uno solo
    ///   y los menús guardados se reapuntan al plato conservado.
    /// </summary>
    public partial class PlatoMultiFranja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Categoria",
                table: "Platoss",
                newName: "Franjas");

            // 0=Desayuno, 1=Almuerzo, 2=Cena  →  1, 2, 4
            migrationBuilder.Sql(
                "UPDATE Platoss SET Franjas = CASE Franjas WHEN 0 THEN 1 WHEN 1 THEN 2 WHEN 2 THEN 4 ELSE Franjas END;");

            // Mapa plato → plato conservado (el de menor Id con el mismo nombre).
            migrationBuilder.Sql(@"
                CREATE TEMP TABLE _MapaPlatos AS
                SELECT p.Id AS OldId,
                       (SELECT MIN(q.Id) FROM Platoss q
                         WHERE lower(trim(q.Nombre)) = lower(trim(p.Nombre))) AS NewId
                FROM Platoss p;");

            // Cada fila tiene un único bit, así que SUM(DISTINCT) equivale a un OR de bits.
            migrationBuilder.Sql(@"
                UPDATE Platoss SET
                    Franjas = (SELECT SUM(DISTINCT q.Franjas) FROM Platoss q
                                WHERE lower(trim(q.Nombre)) = lower(trim(Platoss.Nombre))),
                    Activo  = (SELECT MAX(q.Activo) FROM Platoss q
                                WHERE lower(trim(q.Nombre)) = lower(trim(Platoss.Nombre)))
                WHERE Id IN (SELECT NewId FROM _MapaPlatos WHERE OldId <> NewId);");

            foreach (var columna in new[] { "DesayunoId", "AlmuerzoId", "CenaId" })
            {
                migrationBuilder.Sql($@"
                    UPDATE MenuDias
                    SET {columna} = (SELECT NewId FROM _MapaPlatos WHERE OldId = MenuDias.{columna})
                    WHERE {columna} IN (SELECT OldId FROM _MapaPlatos WHERE OldId <> NewId);");
            }

            migrationBuilder.Sql("DELETE FROM Platoss WHERE Id IN (SELECT OldId FROM _MapaPlatos WHERE OldId <> NewId);");
            migrationBuilder.Sql("DROP TABLE _MapaPlatos;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Aproximación: un plato con varias franjas conserva solo la primera.
            // Los platos fusionados no se vuelven a separar.
            migrationBuilder.Sql(
                "UPDATE Platoss SET Franjas = CASE WHEN Franjas & 1 THEN 0 WHEN Franjas & 2 THEN 1 ELSE 2 END;");

            migrationBuilder.RenameColumn(
                name: "Franjas",
                table: "Platoss",
                newName: "Categoria");
        }
    }
}
