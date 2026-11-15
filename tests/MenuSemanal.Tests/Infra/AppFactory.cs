using System.Net;
using System.Text.RegularExpressions;
using MenuSemanal.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MenuSemanal.Tests.Infra;

/// <summary>
/// Levanta la app completa contra una BD SQLite temporal (nunca la de Data/).
/// Al arrancar se aplican las migraciones y el seed de comidas.json, igual que en producción.
/// </summary>
public class AppFactory : WebApplicationFactory<Program>
{
    private readonly string _dir = Directory.CreateTempSubdirectory("menusemanal-tests-").FullName;

    public string DbPath => Path.Combine(_dir, "menus.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite($"Data Source={DbPath};Pooling=False"));
        });
    }

    /// <summary>
    /// Contexto directo sobre la BD de prueba. Arranca antes la app para que
    /// las migraciones y el seed ya estén aplicados.
    /// </summary>
    public ApplicationDbContext CrearContexto()
    {
        _ = Server;
        return new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite($"Data Source={DbPath};Pooling=False").Options);
    }

    public HttpClient CrearCliente() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
    }
}

public static partial class HttpExtensions
{
    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();

    /// <summary>Hace GET a la página del formulario y envía el POST con su token antiforgery.</summary>
    public static async Task<HttpResponseMessage> EnviarFormularioAsync(
        this HttpClient cliente, string paginaFormulario, string accion, IEnumerable<KeyValuePair<string, string>> campos)
    {
        var html = await cliente.GetStringAsync(paginaFormulario);
        var token = TokenRegex().Match(html).Groups[1].Value;
        Assert.False(string.IsNullOrEmpty(token), $"No se encontró token antiforgery en {paginaFormulario}");

        var datos = campos.Append(new("__RequestVerificationToken", WebUtility.HtmlDecode(token)));
        return await cliente.PostAsync(accion, new FormUrlEncodedContent(datos));
    }

    /// <summary>Sigue una redirección (PRG) y devuelve el HTML resultante.</summary>
    public static async Task<string> SeguirRedireccionAsync(this HttpClient cliente, HttpResponseMessage respuesta)
    {
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        return await cliente.GetStringAsync(respuesta.Headers.Location);
    }
}
