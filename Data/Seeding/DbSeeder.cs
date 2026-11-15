using Microsoft.EntityFrameworkCore;
using MenuSemanal.Models;
using System.Text.Json;

namespace MenuSemanal.Data.Seeding;

/// <summary>
/// Seeder que carga los platos desde comidas.json cuando la BD está vacía.
/// Un nombre que aparece en varias secciones del JSON (ej. "Panqueques" en
/// desayunos y cenas) se guarda como un único plato con varias franjas.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbSeeder));

        if (await ctx.Platos.AnyAsync())
            return;

        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "comidas.json");
        if (!File.Exists(jsonPath))
        {
            logger.LogWarning("comidas.json no encontrado en {Ruta}; se omite la carga inicial.", jsonPath);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        var menuData = JsonSerializer.Deserialize<MenuData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (menuData == null)
        {
            logger.LogWarning("No se pudo deserializar comidas.json; se omite la carga inicial.");
            return;
        }

        var platos = new Dictionary<string, Plato>(StringComparer.OrdinalIgnoreCase);
        var ahora = DateTime.UtcNow;

        void Agregar(IEnumerable<ComidaItem> items, PlatoCategoria franja)
        {
            foreach (var nombre in items.Select(i => i.Nombre.Trim()).Where(n => n.Length > 0))
            {
                if (platos.TryGetValue(nombre, out var existente))
                    existente.Franjas |= franja;
                else
                    platos[nombre] = new Plato { Nombre = nombre, Franjas = franja, FechaCreacion = ahora };
            }
        }

        Agregar(menuData.Desayunos, PlatoCategoria.Desayuno);
        Agregar(menuData.Almuerzos, PlatoCategoria.Almuerzo);
        Agregar(menuData.Cenas, PlatoCategoria.Cena);

        ctx.Platos.AddRange(platos.Values);
        await ctx.SaveChangesAsync();
        logger.LogInformation("{Cantidad} platos cargados desde comidas.json.", platos.Count);
    }
}
