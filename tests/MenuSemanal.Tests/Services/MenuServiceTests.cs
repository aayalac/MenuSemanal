using MenuSemanal.Data;
using MenuSemanal.Models;
using MenuSemanal.Services;
using MenuSemanal.Tests.Infra;

namespace MenuSemanal.Tests.Services;

public class MenuServiceTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private static MenuService Servicio(ApplicationDbContext ctx) => new(ctx, new PlatoService(ctx));

    private void CatalogoBasico()
    {
        _db.AgregarPlato("Huevos", PlatoCategoria.Desayuno);
        _db.AgregarPlato("Panqueques", PlatoCategoria.Desayuno | PlatoCategoria.Cena);
        _db.AgregarPlato("Lentejas", PlatoCategoria.Almuerzo);
        _db.AgregarPlato("Sopa", PlatoCategoria.Almuerzo | PlatoCategoria.Cena);
        _db.AgregarPlato("Hot dog", PlatoCategoria.Cena);
        _db.AgregarPlato("Inactivo", PlatoCategoria.Desayuno | PlatoCategoria.Almuerzo | PlatoCategoria.Cena, activo: false);
    }

    [Fact]
    public async Task Generar_Crea7DiasConPlatosActivosDeSuFranja()
    {
        CatalogoBasico();
        int id;
        using (var ctx = _db.CrearContexto())
            id = (await Servicio(ctx).GenerateWeeklyMenuAsync()).Id;

        using var verificacion = _db.CrearContexto();
        var menu = await Servicio(verificacion).GetMenuByIdAsync(id);

        Assert.Equal([1, 2, 3, 4, 5, 6, 7], menu!.Dias.Select(d => d.DiaSemana).Order());
        Assert.All(menu.Dias, d =>
        {
            Assert.True(d.Desayuno!.Activo && d.Desayuno.TieneFranja(PlatoCategoria.Desayuno));
            Assert.True(d.Almuerzo!.Activo && d.Almuerzo.TieneFranja(PlatoCategoria.Almuerzo));
            Assert.True(d.Cena!.Activo && d.Cena.TieneFranja(PlatoCategoria.Cena));
        });
    }

    [Theory]
    [InlineData("2026-11-16", "2026-11-16")] // lunes
    [InlineData("2026-11-18", "2026-11-16")] // miércoles
    [InlineData("2026-11-22", "2026-11-16")] // domingo
    public async Task Generar_NormalizaLaFechaAlLunes(string fecha, string lunes)
    {
        CatalogoBasico();
        using var ctx = _db.CrearContexto();
        var menu = await Servicio(ctx).GenerateWeeklyMenuAsync(DateOnly.Parse(fecha));
        Assert.Equal(DateOnly.Parse(lunes), menu.SemanaFecha);
    }

    [Fact]
    public async Task Generar_EvitaRepetirPlatoEnElMismoDiaSiHayAlternativa()
    {
        // La sopa es el único almuerzo; como cena también podría salir, pero hay alternativa.
        _db.AgregarPlato("Huevos", PlatoCategoria.Desayuno);
        _db.AgregarPlato("Sopa", PlatoCategoria.Almuerzo | PlatoCategoria.Cena);
        _db.AgregarPlato("Hot dog", PlatoCategoria.Cena);

        for (int i = 0; i < 5; i++)
        {
            using var ctx = _db.CrearContexto();
            var menu = await Servicio(ctx).GenerateWeeklyMenuAsync();
            Assert.All(menu.Dias, d =>
            {
                Assert.Equal("Sopa", d.Almuerzo!.Nombre);
                Assert.Equal("Hot dog", d.Cena!.Nombre);
            });
        }
    }

    [Fact]
    public async Task Generar_RepiteSiNoHayAlternativa()
    {
        _db.AgregarPlato("Huevos", PlatoCategoria.Desayuno);
        _db.AgregarPlato("Sopa", PlatoCategoria.Almuerzo | PlatoCategoria.Cena);

        using var ctx = _db.CrearContexto();
        var menu = await Servicio(ctx).GenerateWeeklyMenuAsync();
        Assert.All(menu.Dias, d => Assert.Equal(d.AlmuerzoId, d.CenaId));
    }

    [Fact]
    public async Task Generar_SinPlatosEnAlgunaFranja_Falla()
    {
        _db.AgregarPlato("Huevos", PlatoCategoria.Desayuno);
        _db.AgregarPlato("Lentejas", PlatoCategoria.Almuerzo);
        _db.AgregarPlato("Hot dog", PlatoCategoria.Cena, activo: false);

        using var ctx = _db.CrearContexto();
        await Assert.ThrowsAsync<InvalidOperationException>(() => Servicio(ctx).GenerateWeeklyMenuAsync());
    }

    [Fact]
    public async Task GetLatest_DevuelveElUltimoGenerado()
    {
        CatalogoBasico();
        int ultimo;
        using (var ctx = _db.CrearContexto())
        {
            await Servicio(ctx).GenerateWeeklyMenuAsync();
            ultimo = (await Servicio(ctx).GenerateWeeklyMenuAsync()).Id;
        }

        using var verificacion = _db.CrearContexto();
        var latest = await Servicio(verificacion).GetLatestMenuAsync();
        Assert.Equal(ultimo, latest!.Id);
        Assert.Equal(2, await Servicio(verificacion).CountAsync());
    }

    [Fact]
    public async Task UpdateMenu_ConPlatosValidos_Guarda()
    {
        CatalogoBasico();
        int id;
        using (var ctx = _db.CrearContexto())
            id = (await Servicio(ctx).GenerateWeeklyMenuAsync()).Id;

        using (var ctx = _db.CrearContexto())
        {
            var panqueques = ctx.Platos.Single(p => p.Nombre == "Panqueques").Id;
            var sopa = ctx.Platos.Single(p => p.Nombre == "Sopa").Id;
            // Panqueques como desayuno y como cena: válido porque tiene ambas franjas.
            Assert.True(await Servicio(ctx).UpdateMenuAsync(id, [new CambioDia(3, panqueques, sopa, panqueques)]));
        }

        using var verificacion = _db.CrearContexto();
        var miercoles = (await Servicio(verificacion).GetMenuByIdAsync(id))!.Dias.Single(d => d.DiaSemana == 3);
        Assert.Equal(("Panqueques", "Sopa", "Panqueques"),
            (miercoles.Desayuno!.Nombre, miercoles.Almuerzo!.Nombre, miercoles.Cena!.Nombre));
    }

    [Fact]
    public async Task UpdateMenu_PlatoFueraDeSuFranja_RechazaSinGuardarNada()
    {
        CatalogoBasico();
        int id;
        using (var ctx = _db.CrearContexto())
            id = (await Servicio(ctx).GenerateWeeklyMenuAsync()).Id;

        using (var ctx = _db.CrearContexto())
        {
            var huevos = ctx.Platos.Single(p => p.Nombre == "Huevos").Id;
            var lentejas = ctx.Platos.Single(p => p.Nombre == "Lentejas").Id;
            // Día 1 válido, día 2 con "Lentejas" como cena (no es cena) → todo se rechaza.
            Assert.False(await Servicio(ctx).UpdateMenuAsync(id,
            [
                new CambioDia(1, huevos, null, null),
                new CambioDia(2, huevos, lentejas, lentejas)
            ]));
        }

        using var verificacion = _db.CrearContexto();
        var lunes = (await Servicio(verificacion).GetMenuByIdAsync(id))!.Dias.Single(d => d.DiaSemana == 1);
        Assert.NotNull(lunes.AlmuerzoId);
    }

    [Fact]
    public async Task UpdateMenu_MenuInexistente_DevuelveFalse()
    {
        using var ctx = _db.CrearContexto();
        Assert.False(await Servicio(ctx).UpdateMenuAsync(999, [new CambioDia(1, null, null, null)]));
    }
}
