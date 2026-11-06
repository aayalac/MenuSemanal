using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin.Menus;

public class ListModel(MenuService menuService) : PageModel
{
    public List<MenuSemanal.Models.MenuSemanario> Menus { get; set; } = [];

    public async Task OnGetAsync()
    {
        Menus = await menuService.GetAllMenusAsync();
    }
}
