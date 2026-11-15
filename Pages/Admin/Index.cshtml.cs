using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin;

public class IndexModel(PlatoService platoService, MenuService menuService) : PageModel
{
    public int TotalPlatos { get; set; }
    public int TotalDesayunos { get; set; }
    public int TotalAlmuerzos { get; set; }
    public int TotalCenas { get; set; }
    public int TotalMenus { get; set; }

    public async Task OnGetAsync()
    {
        var platos = await platoService.GetAllAsync();
        TotalPlatos = platos.Count;
        TotalDesayunos = platos.Count(p => p.TieneFranja(PlatoCategoria.Desayuno));
        TotalAlmuerzos = platos.Count(p => p.TieneFranja(PlatoCategoria.Almuerzo));
        TotalCenas = platos.Count(p => p.TieneFranja(PlatoCategoria.Cena));
        TotalMenus = await menuService.CountAsync();
    }
}
