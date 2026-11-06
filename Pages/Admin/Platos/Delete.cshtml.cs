using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin.Platos;

public class DeleteModel(PlatoService platoService) : PageModel
{
    public Plato? Plato { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Plato = await platoService.GetByIdAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var ok = await platoService.DeleteAsync(id);
        if (ok)
        {
            TempData["SuccessMsg"] = "Plato eliminado correctamente.";
        }
        return RedirectToPage("./Index");
    }
}
