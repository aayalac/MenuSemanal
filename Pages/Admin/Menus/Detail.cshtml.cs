using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin.Menus;

public class DetailModel(MenuService menuService) : PageModel
{
    public MenuSemanal.Models.MenuSemanario? Menu { get; set; }

    public async Task OnGetAsync(int id)
    {
        Menu = await menuService.GetMenuByIdAsync(id);
    }
}
