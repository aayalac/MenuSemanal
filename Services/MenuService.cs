using Microsoft.EntityFrameworkCore;
using MenuSemanal.Data;
using MenuSemanal.Models;
using MenuSemanario = MenuSemanal.Models.MenuSemanario;

namespace MenuSemanal.Services;

/// <summary>
/// Servicio para generar y gestionar menús semanales con persistencia en BD.
/// Reemplaza al antiguo MenuService basado en JSON.
/// </summary>
public class MenuService(ApplicationDbContext dbContext, PlatoService platoService)
{
    private static readonly string[] DiasSemana =
    [
        "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"
    ];

    /// <summary>
    /// Genera un nuevo menú semanal usando platos aleatorios activos de la BD.
    /// Guarda el resultado permanentemente.
    /// </summary>
    public async Task<MenuSemanario> GenerateWeeklyMenuAsync(DateOnly? fechaBase = null)
    {
        var (desayunos, almuerzos, cenas) = await platoService.GetActiveNamesByCategoryAsync();

        if (desayunos.Count == 0 || almuerzos.Count == 0 || cenas.Count == 0)
            throw new InvalidOperationException("Debe haber al menos un plato activo en cada categoría.");

        var rng = new Random();
        var menu = new MenuSemanario
        {
            SemanaFecha = fechaBase ?? DateOnly.FromDateTime(DateTime.Today)
        };

        for (int i = 0; i < 7; i++)
        {
            var dia = new MenuDia
            {
                DiaSemana = i + 1
            };

            // Buscar plato por nombre (ya existen porque están en la BD)
            dia.Desayuno = await ObtenerOAlternativoAsync(desayunos[rng.Next(desayunos.Count)], PlatoCategoria.Desayuno);
            dia.Almuerzo = await ObtenerOAlternativoAsync(almuerzos[rng.Next(almuerzos.Count)], PlatoCategoria.Almuerzo);
            dia.Cena = await ObtenerOAlternativoAsync(cenas[rng.Next(cenas.Count)], PlatoCategoria.Cena);

            menu.Dias.Add(dia);
        }

        dbContext.MenuSemanars.Add(menu);
        await dbContext.SaveChangesAsync();
        return menu;
    }

    /// <summary>
    /// Si el plato ya existe en BD usa ese, si no crea uno temporal.
    /// Nota: usamos AsEnumerable() porque SQLite EF Core provider no traduce
    /// String.Equals/StringComparer.OrdinalIgnoreCase a SQL. Con ~64 platos,
    /// cargarlos en memoria es perfectamente viable.
    /// </summary>
    private async Task<Plato> ObtenerOAlternativoAsync(string nombre, PlatoCategoria cat)
    {
        // Primero intentamos en la BD (client-eval tras filtro ligero de categoría)
        var candidatos = dbContext.Platoss
            .Where(p => p.Categoria == cat && p.Activo)
            .AsEnumerable()
            .FirstOrDefault(p =>
                string.Equals(p.Nombre, nombre, StringComparison.OrdinalIgnoreCase));

        if (candidatos != null)
            return candidatos;

        // Si no existe aún, lo creamos (caso edge: plato eliminado recientemente)
        var nuevo = new Plato
        {
            Nombre = nombre,
            Categoria = cat
        };
        dbContext.Platoss.Add(nuevo);
        await dbContext.SaveChangesAsync();
        return nuevo;
    }

    public async Task<List<MenuSemanario>> GetAllMenusAsync() =>
        await dbContext.MenuSemanars.OrderByDescending(m => m.SemanaFecha).ThenByDescending(m => m.FechaCreacion).ToListAsync();

    public async Task<MenuSemanario?> GetMenuByIdAsync(int id) =>
        await dbContext.MenuSemanars.Include(m => m.Dias).FirstOrDefaultAsync(m => m.Id == id);

    public async Task DeleteMenuAsync(int id)
    {
        var menu = await dbContext.MenuSemanars.FindAsync(id);
        if (menu != null)
        {
            dbContext.MenuSemanars.Remove(menu);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Actualiza un plato específico de un día en un menú semanal dado.
    /// Útil para permitir ajustes manuales post-generación.
    /// </summary>
    public async Task<bool> UpdateDayMealAsync(int menuId, int diaSemana, PlatoCategoria tipo, string nuevoPlatoNombre)
    {
        var menu = await dbContext.MenuSemanars
            .Include(m => m.Dias)
            .FirstOrDefaultAsync(m => m.Id == menuId);

        if (menu == null) return false;

        var dia = menu.Dias.FirstOrDefault(d => d.DiaSemana == diaSemana);
        if (dia == null) return false;

        var plato = dbContext.Platoss
            .Where(p => p.Categoria == tipo && p.Activo)
            .AsEnumerable()
            .FirstOrDefault(p =>
                string.Equals(p.Nombre, nuevoPlatoNombre, StringComparison.OrdinalIgnoreCase));

        if (plato == null)
        {
            plato = new Plato { Nombre = nuevoPlatoNombre, Categoria = tipo };
            dbContext.Platoss.Add(plato);
            await dbContext.SaveChangesAsync();
        }

        switch (tipo)
        {
            case PlatoCategoria.Desayuno:
                dia.DesayunoId = plato.Id;
                break;
            case PlatoCategoria.Almuerzo:
                dia.AlmuerzoId = plato.Id;
                break;
            case PlatoCategoria.Cena:
                dia.CenaId = plato.Id;
                break;
        }

        await dbContext.SaveChangesAsync();
        return true;
    }

    #region Legacy compatibility wrappers

    /// <summary>
    /// Wrapper backward-compatible que devuelve listas simples de texto.
    /// Para mantener compatibilidad con la página Index antigua.
    /// </summary>
    public async Task<List<DiaMenu>> GenerateAndReturnSimpleAsync(DateOnly? fechaBase = null)
    {
        var menu = await GenerateWeeklyMenuAsync(fechaBase);
        return menu.Dias.Select(d => new DiaMenu
        {
            Dia = d.DiaNombre,
            Desayuno = d.Desayuno?.Nombre,
            Almuerzo = d.Almuerzo?.Nombre,
            Cena = d.Cena?.Nombre
        }).ToList();
    }

    public async Task<string> GetRandomDesayunoAsync()
    {
        var items = await platoService.GetByCategoryAsync(PlatoCategoria.Desayuno);
        return items[new Random().Next(items.Count)].Nombre;
    }

    public async Task<string> GetRandomAlmuerzoAsync()
    {
        var items = await platoService.GetByCategoryAsync(PlatoCategoria.Almuerzo);
        return items[new Random().Next(items.Count)].Nombre;
    }

    public async Task<string> GetRandomCenaAsync()
    {
        var items = await platoService.GetByCategoryAsync(PlatoCategoria.Cena);
        return items[new Random().Next(items.Count)].Nombre;
    }

    #endregion
}
