using MenuSemanal.Models;
using MenuSemanal.Services;
using MenuSemanal.Tests.Infra;

namespace MenuSemanal.Tests.Services;

public class PlatoServiceTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetActivosPorFranja_IncluyePlatosMultiFranjaYExcluyeInactivos()
    {
        _db.AgregarPlato("Sopa", PlatoCategoria.Almuerzo | PlatoCategoria.Cena);
        _db.AgregarPlato("Hot dog", PlatoCategoria.Cena);
        _db.AgregarPlato("Lentejas", PlatoCategoria.Almuerzo);
        _db.AgregarPlato("Choripán", PlatoCategoria.Cena, activo: false);

        using var ctx = _db.CrearContexto();
        var cenas = await new PlatoService(ctx).GetActivosPorFranjaAsync(PlatoCategoria.Cena);

        Assert.Equal(["Hot dog", "Sopa"], cenas.Select(p => p.Nombre));
    }

    [Theory]
    [InlineData("Sopa", true)]
    [InlineData("  sOPa ", true)]
    [InlineData("Sopas", false)]
    public async Task ExisteNombre_NoDistingueMayusculasNiEspacios(string nombre, bool esperado)
    {
        _db.AgregarPlato("Sopa", PlatoCategoria.Almuerzo);
        using var ctx = _db.CrearContexto();

        Assert.Equal(esperado, await new PlatoService(ctx).ExisteNombreAsync(nombre));
    }

    [Fact]
    public async Task ExisteNombre_IgnoraElPropioPlatoAlEditar()
    {
        var sopa = _db.AgregarPlato("Sopa", PlatoCategoria.Almuerzo);
        using var ctx = _db.CrearContexto();

        Assert.False(await new PlatoService(ctx).ExisteNombreAsync("SOPA", excluirId: sopa.Id));
    }

    [Fact]
    public async Task Update_GuardaNombreFranjasYEstado()
    {
        var sopa = _db.AgregarPlato("Sopa", PlatoCategoria.Almuerzo);

        using (var ctx = _db.CrearContexto())
            Assert.True(await new PlatoService(ctx).UpdateAsync(
                sopa.Id, "  Sopa de verduras ", PlatoCategoria.Almuerzo | PlatoCategoria.Cena, activo: false));

        using var verificacion = _db.CrearContexto();
        var guardado = await verificacion.Platos.FindAsync(sopa.Id);
        Assert.Equal("Sopa de verduras", guardado!.Nombre);
        Assert.Equal(PlatoCategoria.Almuerzo | PlatoCategoria.Cena, guardado.Franjas);
        Assert.False(guardado.Activo);
    }

    [Fact]
    public async Task DeleteOrDeactivate_SinUso_EliminaElPlato()
    {
        var plato = _db.AgregarPlato("Arepa", PlatoCategoria.Desayuno);
        using var ctx = _db.CrearContexto();

        Assert.True(await new PlatoService(ctx).DeleteOrDeactivateAsync(plato.Id));
        Assert.Null(await ctx.Platos.FindAsync(plato.Id));
    }

    [Fact]
    public async Task DeleteOrDeactivate_UsadoEnMenu_SoloDesactivaYConservaHistorial()
    {
        var huevos = _db.AgregarPlato("Huevos", PlatoCategoria.Desayuno);
        _db.AgregarPlato("Lentejas", PlatoCategoria.Almuerzo);
        _db.AgregarPlato("Hot dog", PlatoCategoria.Cena);
        int menuId;
        using (var ctx = _db.CrearContexto())
            menuId = (await new MenuService(ctx, new PlatoService(ctx)).GenerateWeeklyMenuAsync()).Id;

        using (var ctx = _db.CrearContexto())
            Assert.False(await new PlatoService(ctx).DeleteOrDeactivateAsync(huevos.Id));

        using var verificacion = _db.CrearContexto();
        Assert.False((await verificacion.Platos.FindAsync(huevos.Id))!.Activo);
        var menu = await new MenuService(verificacion, new PlatoService(verificacion)).GetMenuByIdAsync(menuId);
        Assert.All(menu!.Dias, d => Assert.Equal(huevos.Id, d.DesayunoId));
    }

    [Fact]
    public async Task DeleteOrDeactivate_Inexistente_DevuelveNull()
    {
        using var ctx = _db.CrearContexto();
        Assert.Null(await new PlatoService(ctx).DeleteOrDeactivateAsync(999));
    }
}
