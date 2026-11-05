using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin.Menus;

public class DeleteConfirmModel(MenuService menuService) : PageModel
{
    [BindProperty]
    public int Id { get; set; }
    public MenuSemanal.Models.MenuSemanario? Menu { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Menu = await menuService.GetMenuByIdAsync(id);
        if (Menu == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await menuService.DeleteMenuAsync(Id);
        TempData["SuccessMsg"] = "Menú eliminado correctamente.";
        return RedirectToPage("../Menus/List");
    }
}
