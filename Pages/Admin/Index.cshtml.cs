using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin;

public class IndexModel(PlatoService platoService, MenuService menuService) : PageModel
{
    public int TotalPlatoss { get; set; }
    public int TotalDesayunos { get; set; }
    public int TotalAlmuerzos { get; set; }
    public int TotalCenas { get; set; }
    public int TotalMenus { get; set; }

    public async Task OnGetAsync()
    {
        var platoss = await platoService.GetAllAsync();
        TotalPlatoss = platoss.Count;
        TotalDesayunos = platoss.Count(p => p.Categoria == Models.PlatoCategoria.Desayuno);
        TotalAlmuerzos = platoss.Count(p => p.Categoria == Models.PlatoCategoria.Almuerzo);
        TotalCenas = platoss.Count(p => p.Categoria == Models.PlatoCategoria.Cena);
        TotalMenus = (await menuService.GetAllMenusAsync()).Count;
    }
}
