using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages;

public class IndexModel : PageModel
{
    private readonly MenuService _menuService;

    public IndexModel(MenuService menuService)
    {
        _menuService = menuService;
    }

    [BindProperty]
    public List<DiaMenu> WeeklyMenu { get; set; } = new();

    public async Task OnGetAsync()
    {
        WeeklyMenu = await _menuService.GenerateWeeklyMenuAsync();
    }

    public async Task<IActionResult> OnPostGenerarAsync()
    {
        WeeklyMenu = await _menuService.GenerateWeeklyMenuAsync();
        ModelState.Clear();
        return Page();
    }
}
