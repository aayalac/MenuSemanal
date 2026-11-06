using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages;

public class IndexModel(MenuService menuService) : PageModel
{
    private readonly MenuService _menuService = menuService;

    [BindProperty]
    public List<DiaMenu> WeeklyMenu { get; set; } = [];

    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        // Mostrar el menú más reciente si ya existe
        var menus = await _menuService.GetAllMenusAsync();
        if (menus.Any())
        {
            var latest = menus.First();
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
        await GenerateNewMenuAsync();
        SuccessMessage = "✨ ¡Menú semanal generado exitosamente!";
        ModelState.Clear();
        return Page();
    }

    private async Task GenerateNewMenuAsync()
    {
        var menu = await _menuService.GenerateWeeklyMenuAsync();
        WeeklyMenu = menu.Dias.OrderBy(d => d.DiaSemana).Select(d => new DiaMenu
        {
            Dia = d.DiaNombre,
            Desayuno = d.Desayuno?.Nombre,
            Almuerzo = d.Almuerzo?.Nombre,
            Cena = d.Cena?.Nombre
        }).ToList();
    }
}
