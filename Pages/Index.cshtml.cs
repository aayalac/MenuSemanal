using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages;

public class IndexModel(MenuService menuService, ILogger<IndexModel> logger) : PageModel
{
    private readonly MenuService _menuService = menuService;
    private readonly ILogger<IndexModel> _logger = logger;

    [BindProperty]
    public List<DiaMenu> WeeklyMenu { get; set; } = [];

    /// <summary>Lunes de la semana a la que pertenece el menú mostrado.</summary>
    public DateOnly? SemanaFecha { get; set; }

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        // Mostrar el menú más reciente si ya existe
        var menus = await _menuService.GetAllMenusAsync();
        if (menus.Any())
        {
            var latest = menus.First();
            SemanaFecha = latest.SemanaFecha;
            WeeklyMenu = latest.Dias.OrderBy(d => d.DiaSemana).Select(d => new DiaMenu
            {
                Dia = d.DiaNombre,
                Desayuno = d.Desayuno?.Nombre,
                Almuerzo = d.Almuerzo?.Nombre,
                Cena = d.Cena?.Nombre
            }).ToList();
        }
        else
        {
            // Primera visita: generar un menú
            await GenerateNewMenuAsync();
        }
    }

    public async Task<IActionResult> OnPostGenerarAsync()
    {
        try
        {
            await GenerateNewMenuAsync();
            SuccessMessage = "✨ ¡Menú semanal generado y guardado en el historial!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar el menú semanal");
            ErrorMessage = "No se pudo generar el menú. Revisá que exista al menos un plato activo por categoría.";
        }

        ModelState.Clear();
        return Page();
    }

    private async Task GenerateNewMenuAsync()
    {
        var menu = await _menuService.GenerateWeeklyMenuAsync();
        SemanaFecha = menu.SemanaFecha;
        WeeklyMenu = menu.Dias.OrderBy(d => d.DiaSemana).Select(d => new DiaMenu
        {
            Dia = d.DiaNombre,
            Desayuno = d.Desayuno?.Nombre,
            Almuerzo = d.Almuerzo?.Nombre,
            Cena = d.Cena?.Nombre
        }).ToList();
    }
}
