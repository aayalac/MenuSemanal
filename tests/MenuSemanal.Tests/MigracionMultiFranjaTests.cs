using MenuSemanal.Models;
using MenuSemanal.Tests.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MenuSemanal.Tests;

/// <summary>
/// Verifica la migración de datos del issue #5 partiendo de una BD con el esquema anterior.
/// </summary>
public class MigracionMultiFranjaTests
{
    [Fact]
    public void FusionaDuplicadosYReapuntaMenus()
    {
        using var db = new TestDb(migrar: false);
        using (var ctx = db.CrearContexto())
            ctx.GetService<IMigrator>().Migrate("MenuPorSemanaNoUnico");

        // Esquema anterior: Categoria 0=Desayuno, 1=Almuerzo, 2=Cena; nombres repetidos por categoría.
        Ejecutar(db, """
            INSERT INTO Platoss (Id, Nombre, Categoria, FechaCreacion, Activo) VALUES
                (1, 'Panqueques', 0, '2026-01-01', 1),
                (2, 'Lentejas',   1, '2026-01-01', 1),
                (3, 'panqueques', 2, '2026-01-01', 0),
                (4, 'Hot dog',    2, '2026-01-01', 1);
            INSERT INTO MenuSemanars (Id, SemanaFecha, FechaCreacion) VALUES (1, '2026-11-16', '2026-11-16');
            INSERT INTO MenuDias (MenuSemanalId, DiaSemana, DesayunoId, AlmuerzoId, CenaId) VALUES
                (1, 1, 1, 2, 3),
                (1, 2, 1, 2, 4);
            """);

        using (var ctx = db.CrearContexto())
            ctx.Database.Migrate();

        using var verificacion = db.CrearContexto();
        var platos = verificacion.Platos.OrderBy(p => p.Id).ToList();
        Assert.Equal([1, 2, 4], platos.Select(p => p.Id));
        Assert.Equal(PlatoCategoria.Desayuno | PlatoCategoria.Cena, platos[0].Franjas);
        Assert.True(platos[0].Activo);
        Assert.Equal(PlatoCategoria.Almuerzo, platos[1].Franjas);
        Assert.Equal(PlatoCategoria.Cena, platos[2].Franjas);

        var dias = verificacion.MenuDias.OrderBy(d => d.DiaSemana).ToList();
        Assert.Equal(1, dias[0].CenaId); // antes apuntaba al duplicado eliminado (3)
        Assert.Equal(4, dias[1].CenaId);
        Assert.Equal(1, verificacion.MenusSemanales.Count());
    }

    private static void Ejecutar(TestDb db, string sql)
    {
        using var cmd = db.Conexion.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
