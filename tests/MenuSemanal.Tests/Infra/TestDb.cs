using MenuSemanal.Data;
using MenuSemanal.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MenuSemanal.Tests.Infra;

/// <summary>
/// BD SQLite en memoria con las migraciones reales aplicadas.
/// Se usa el proveedor real (no InMemory) para detectar consultas que SQLite no puede traducir.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _conexion;

    public TestDb(bool migrar = true)
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();
        if (migrar)
        {
            using var ctx = CrearContexto();
            ctx.Database.Migrate();
        }
    }

    public SqliteConnection Conexion => _conexion;

    /// <summary>Contexto nuevo (sin entidades en caché) sobre la misma BD.</summary>
    public ApplicationDbContext CrearContexto() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_conexion).Options);

    public Plato AgregarPlato(string nombre, PlatoCategoria franjas, bool activo = true)
    {
        using var ctx = CrearContexto();
        var plato = new Plato { Nombre = nombre, Franjas = franjas, Activo = activo };
        ctx.Platos.Add(plato);
        ctx.SaveChanges();
        return plato;
    }

    public void Dispose() => _conexion.Dispose();
}
