using Microsoft.EntityFrameworkCore;
using MenuSemanal.Data;
using MenuSemanal.Models;

namespace MenuSemanal.Services;

/// <summary>
/// Servicio para generar y gestionar menús semanales con persistencia en BD.
/// </summary>
public class MenuService(ApplicationDbContext dbContext, PlatoService platoService)
{
    /// <summary>
    /// Genera un nuevo menú semanal usando platos activos aleatorios y lo guarda.
    /// Si es posible, evita repetir el mismo plato en dos franjas del mismo día
    /// (un plato puede pertenecer a varias franjas, p. ej. Almuerzo y Cena).
    /// </summary>
    public async Task<MenuSemanario> GenerateWeeklyMenuAsync(DateOnly? fechaBase = null)
    {
        var desayunos = await platoService.GetActivosPorFranjaAsync(PlatoCategoria.Desayuno);
        var almuerzos = await platoService.GetActivosPorFranjaAsync(PlatoCategoria.Almuerzo);
        var cenas = await platoService.GetActivosPorFranjaAsync(PlatoCategoria.Cena);

        if (desayunos.Count == 0 || almuerzos.Count == 0 || cenas.Count == 0)
            throw new InvalidOperationException("Debe haber al menos un plato activo en cada franja.");

        // La fecha se normaliza al lunes de la semana, para poder agrupar el historial
        // por semana. No se borra nada: cada generación queda registrada como un menú más.
        var menu = new MenuSemanario
        {
            SemanaFecha = InicioDeSemana(fechaBase ?? DateOnly.FromDateTime(DateTime.Today)),
            FechaCreacion = DateTime.UtcNow
        };

        for (int dia = 1; dia <= 7; dia++)
        {
            var usados = new HashSet<int>();
            var desayuno = Elegir(desayunos, usados);
            var almuerzo = Elegir(almuerzos, usados);
            var cena = Elegir(cenas, usados);

            menu.Dias.Add(new MenuDia
            {
                DiaSemana = dia,
                Desayuno = desayuno,
                Almuerzo = almuerzo,
                Cena = cena
            });
        }

        dbContext.MenusSemanales.Add(menu);
        await dbContext.SaveChangesAsync();
        return menu;
    }

    private static Plato Elegir(List<Plato> candidatos, HashSet<int> usados)
    {
        var disponibles = candidatos.Where(p => !usados.Contains(p.Id)).ToList();
        if (disponibles.Count == 0) disponibles = candidatos;

        var plato = disponibles[Random.Shared.Next(disponibles.Count)];
        usados.Add(plato.Id);
        return plato;
    }

    /// <summary>
    /// Lunes de la semana a la que pertenece la fecha indicada.
    /// </summary>
    private static DateOnly InicioDeSemana(DateOnly fecha)
    {
        // DayOfWeek: Domingo = 0 ... Sábado = 6 → queremos Lunes = 0
        var offset = ((int)fecha.DayOfWeek + 6) % 7;
        return fecha.AddDays(-offset);
    }

    private IQueryable<MenuSemanario> ConPlatos() =>
        dbContext.MenusSemanales
            .Include(m => m.Dias).ThenInclude(d => d.Desayuno)
            .Include(m => m.Dias).ThenInclude(d => d.Almuerzo)
            .Include(m => m.Dias).ThenInclude(d => d.Cena)
            .AsSplitQuery();

    public Task<List<MenuSemanario>> GetAllMenusAsync() =>
        dbContext.MenusSemanales
            .Include(m => m.Dias)
            .OrderByDescending(m => m.SemanaFecha)
            .ThenByDescending(m => m.FechaCreacion)
            .ToListAsync();

    /// <summary>El menú generado más recientemente (o null si no hay ninguno).</summary>
    public Task<MenuSemanario?> GetLatestMenuAsync() =>
        ConPlatos()
            .OrderByDescending(m => m.SemanaFecha)
            .ThenByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

    public Task<MenuSemanario?> GetMenuByIdAsync(int id) =>
        ConPlatos().FirstOrDefaultAsync(m => m.Id == id);

    public Task<int> CountAsync() => dbContext.MenusSemanales.CountAsync();

    public async Task DeleteMenuAsync(int id)
    {
        var menu = await dbContext.MenusSemanales.FindAsync(id);
        if (menu != null)
        {
            dbContext.MenusSemanales.Remove(menu);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Guarda los cambios manuales de un menú (todos los días en una sola operación).
    /// Solo acepta platos que existan y pertenezcan a la franja correspondiente;
    /// no se exige que estén activos, para poder conservar la elección previa.
    /// </summary>
    /// <returns>false si el menú no existe o algún plato no es válido para su franja.</returns>
    public async Task<bool> UpdateMenuAsync(int menuId, IEnumerable<CambioDia> cambios)
    {
        var dias = await dbContext.MenuDias.Where(d => d.MenuSemanalId == menuId).ToListAsync();
        if (dias.Count == 0) return false;

        var franjasPorPlato = await dbContext.Platos.ToDictionaryAsync(p => p.Id, p => p.Franjas);
        bool Valido(int? id, PlatoCategoria franja) =>
            id == null || (franjasPorPlato.TryGetValue(id.Value, out var f) && (f & franja) == franja);

        foreach (var c in cambios)
        {
            var dia = dias.FirstOrDefault(d => d.DiaSemana == c.DiaSemana);
            if (dia == null ||
                !Valido(c.DesayunoId, PlatoCategoria.Desayuno) ||
                !Valido(c.AlmuerzoId, PlatoCategoria.Almuerzo) ||
                !Valido(c.CenaId, PlatoCategoria.Cena))
                return false;

            dia.DesayunoId = c.DesayunoId;
            dia.AlmuerzoId = c.AlmuerzoId;
            dia.CenaId = c.CenaId;
        }

        await dbContext.SaveChangesAsync();
        return true;
    }
}

public record CambioDia(int DiaSemana, int? DesayunoId, int? AlmuerzoId, int? CenaId);
