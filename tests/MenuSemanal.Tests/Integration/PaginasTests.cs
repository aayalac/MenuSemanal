using System.Net;
using MenuSemanal.Models;
using MenuSemanal.Tests.Infra;
using Microsoft.EntityFrameworkCore;

namespace MenuSemanal.Tests.Integration;

public class PaginasTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _app;

    public PaginasTests(AppFactory app) => _app = app;

    private static KeyValuePair<string, string> C(string k, object v) => new(k, v.ToString()!);

    [Fact]
    public async Task Arranque_CargaCatalogoFusionandoNombresRepetidos()
    {
        using var cliente = _app.CrearCliente();
        Assert.Equal("\"healthy\"", await cliente.GetStringAsync("/health"));

        using var ctx = _app.CrearContexto();
        var panqueques = await ctx.Platos.SingleAsync(p => p.Nombre == "Panqueques");
        Assert.Equal(PlatoCategoria.Desayuno | PlatoCategoria.Cena, panqueques.Franjas);
        Assert.Equal(1, await ctx.Platos.CountAsync(p => p.Nombre == "Caldito de pollo"));
    }

    [Fact]
    public async Task Inicio_MuestraMenuEditableCon21Desplegables()
    {
        using var cliente = _app.CrearCliente();
        var html = await cliente.GetStringAsync("/");

        Assert.Equal(21, html.Split("<select").Length - 1);
        Assert.DoesNotContain("disabled", html);
        Assert.Contains("Guardar Cambios", html);
    }

    [Theory]
    [InlineData("/Admin")]
    [InlineData("/Admin/Platos")]
    [InlineData("/Admin/Platos?categoria=Cena")]
    [InlineData("/Admin/Platos/Create")]
    [InlineData("/Admin/Menus/List")]
    public async Task PaginasDeAdministracion_Responden200(string url)
    {
        using var cliente = _app.CrearCliente();
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task Generar_CreaUnMenuNuevoYRedirige()
    {
        using var cliente = _app.CrearCliente();
        await cliente.GetStringAsync("/"); // asegura un menú inicial
        int antes;
        using (var ctx = _app.CrearContexto()) antes = await ctx.MenusSemanales.CountAsync();

        var resp = await cliente.EnviarFormularioAsync("/", "/?handler=Generar", []);
        var html = await cliente.SeguirRedireccionAsync(resp);

        Assert.Contains("generado y guardado", WebUtility.HtmlDecode(html));
        using var verificacion = _app.CrearContexto();
        Assert.Equal(antes + 1, await verificacion.MenusSemanales.CountAsync());
    }

    [Fact]
    public async Task Guardar_CambiosValidosSePersisten_EInvalidosSeRechazan()
    {
        using var cliente = _app.CrearCliente();
        await cliente.GetStringAsync("/");

        int menuId, panqueques, lentejas;
        using (var ctx = _app.CrearContexto())
        {
            menuId = (await ctx.MenusSemanales.OrderByDescending(m => m.SemanaFecha).ThenByDescending(m => m.FechaCreacion).FirstAsync()).Id;
            panqueques = (await ctx.Platos.SingleAsync(p => p.Nombre == "Panqueques")).Id;
            lentejas = (await ctx.Platos.SingleAsync(p => p.Nombre == "Lentejas")).Id;
        }

        // Válido: Panqueques es desayuno y cena.
        var ok = await cliente.EnviarFormularioAsync("/", "/?handler=Guardar",
        [
            C("MenuId", menuId), C("Dias[0].DiaSemana", 1),
            C("Dias[0].DesayunoId", panqueques), C("Dias[0].AlmuerzoId", lentejas), C("Dias[0].CenaId", panqueques)
        ]);
        Assert.Contains("Cambios guardados", WebUtility.HtmlDecode(await cliente.SeguirRedireccionAsync(ok)));

        using (var ctx = _app.CrearContexto())
        {
            var lunes = await ctx.MenuDias.SingleAsync(d => d.MenuSemanalId == menuId && d.DiaSemana == 1);
            Assert.Equal((panqueques, lentejas, panqueques), (lunes.DesayunoId!.Value, lunes.AlmuerzoId!.Value, lunes.CenaId!.Value));
        }

        // Inválido: Panqueques no es almuerzo.
        var mal = await cliente.EnviarFormularioAsync("/", "/?handler=Guardar",
        [
            C("MenuId", menuId), C("Dias[0].DiaSemana", 1),
            C("Dias[0].DesayunoId", panqueques), C("Dias[0].AlmuerzoId", panqueques), C("Dias[0].CenaId", panqueques)
        ]);
        Assert.Contains("No se pudieron guardar", WebUtility.HtmlDecode(await cliente.SeguirRedireccionAsync(mal)));
    }

    [Fact]
    public async Task CrearPlato_ConVariasFranjas_YRechazaDuplicadosYSinFranja()
    {
        using var cliente = _app.CrearCliente();
        const string form = "/Admin/Platos/Create";

        var creado = await cliente.EnviarFormularioAsync(form, form,
            [C("Plato.Nombre", "Sopa de prueba"), C("Plato.Franjas", "Almuerzo"), C("Plato.Franjas", "Cena")]);
        Assert.Contains("agregado correctamente", await creado.Content.ReadAsStringAsync());

        using (var ctx = _app.CrearContexto())
            Assert.Equal(PlatoCategoria.Almuerzo | PlatoCategoria.Cena,
                (await ctx.Platos.SingleAsync(p => p.Nombre == "Sopa de prueba")).Franjas);

        var duplicado = await cliente.EnviarFormularioAsync(form, form,
            [C("Plato.Nombre", "SOPA DE PRUEBA"), C("Plato.Franjas", "Cena")]);
        Assert.Contains("Ya existe", WebUtility.HtmlDecode(await duplicado.Content.ReadAsStringAsync()));

        var sinFranja = await cliente.EnviarFormularioAsync(form, form, [C("Plato.Nombre", "Arepa de prueba")]);
        Assert.Contains("Selecciona al menos una franja", await sinFranja.Content.ReadAsStringAsync());

        using var verificacion = _app.CrearContexto();
        Assert.Equal(1, await verificacion.Platos.CountAsync(p => p.Nombre.ToLower() == "sopa de prueba"));
        Assert.False(await verificacion.Platos.AnyAsync(p => p.Nombre == "Arepa de prueba"));
    }

    [Fact]
    public async Task EditarPlato_GuardaLosCambios()
    {
        int id;
        using (var ctx = _app.CrearContexto())
        {
            var plato = new Plato { Nombre = "Para editar", Franjas = PlatoCategoria.Desayuno };
            ctx.Platos.Add(plato);
            await ctx.SaveChangesAsync();
            id = plato.Id;
        }

        using var cliente = _app.CrearCliente();
        var resp = await cliente.EnviarFormularioAsync($"/Admin/Platos/Edit/{id}", $"/Admin/Platos/Edit/{id}",
        [
            C("Plato.Nombre", "Editado"), C("Plato.Franjas", "Almuerzo"), C("Plato.Franjas", "Cena"),
            C("Plato.Activo", "false")
        ]);
        var html = WebUtility.HtmlDecode(await cliente.SeguirRedireccionAsync(resp));
        Assert.Contains("actualizado correctamente", html);

        using var verificacion = _app.CrearContexto();
        var guardado = await verificacion.Platos.FindAsync(id);
        Assert.Equal("Editado", guardado!.Nombre);
        Assert.Equal(PlatoCategoria.Almuerzo | PlatoCategoria.Cena, guardado.Franjas);
        Assert.False(guardado.Activo);
    }

    [Fact]
    public async Task EliminarPlato_UsadoEnMenu_SeDesactiva()
    {
        using var cliente = _app.CrearCliente();
        await cliente.GetStringAsync("/");

        int id;
        using (var ctx = _app.CrearContexto())
            id = (await ctx.MenuDias.FirstAsync(d => d.DesayunoId != null)).DesayunoId!.Value;

        var resp = await cliente.EnviarFormularioAsync($"/Admin/Platos/Delete/{id}", $"/Admin/Platos/Delete/{id}", []);
        Assert.Contains("se desactivó", WebUtility.HtmlDecode(await cliente.SeguirRedireccionAsync(resp)));

        using var verificacion = _app.CrearContexto();
        Assert.False((await verificacion.Platos.FindAsync(id))!.Activo);
    }
}
