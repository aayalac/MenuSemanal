using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MenuSemanal.Data;
using MenuSemanal.Models;
using System.Text.Json;

namespace MenuSemanal.Data.Seeding;

/// <summary>
/// Seeder que carga los platos desde comidas.json cuando la BD está vacía.
/// Se ejecuta una sola vez durante la primera migración/aplicación.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Solo sembrar si la tabla de platoss está vacía
        if (ctx.Platoss.Any())
            return;

        Console.WriteLine("[DbSeeder] Tabla de platoss vacía, cargando desde comidas.json...");

        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "Data", "comidas.json");
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine("[DbSeeder] Warning: comidas.json no encontrado, saltando seeding.");
            return;
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var menuData = JsonSerializer.Deserialize<MenuData>(json, opciones);

        if (menuData == null)
        {
            Console.WriteLine("[DbSeeder] Warning: errores al deserialize comidas.json, saltando seeding.");
            return;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        foreach (var item in menuData.Desayunos)
        {
            ctx.Platoss.Add(new Plato
            {
                Nombre = item.Nombre,
                Categoria = PlatoCategoria.Desayuno,
                FechaCreacion = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime
            });
        }

        foreach (var item in menuData.Almuerzos)
        {
            ctx.Platoss.Add(new Plato
            {
                Nombre = item.Nombre,
                Categoria = PlatoCategoria.Almuerzo,
                FechaCreacion = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime
            });
        }

        foreach (var item in menuData.Cenas)
        {
            ctx.Platoss.Add(new Plato
            {
                Nombre = item.Nombre,
                Categoria = PlatoCategoria.Cena,
                FechaCreacion = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime
            });
        }

        await ctx.SaveChangesAsync();
        Console.WriteLine($"[DbSeeder] ✓ {ctx.Platoss.Count()} platos insertados correctamente.");
    }
}
