using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin.Platos;

public class DeleteModel(PlatoService platoService) : PageModel
{
    public Plato? Plato { get; set; }

    /// <summary>Si el plato ya está en algún menú, solo se desactivará.</summary>
    public bool EnUso { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Plato = await platoService.GetByIdAsync(id);
        if (Plato != null)
            EnUso = await platoService.EstaEnUsoAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        TempData["SuccessMsg"] = await platoService.DeleteOrDeactivateAsync(id) switch
        {
            true => "Plato eliminado correctamente.",
            false => "El plato se usa en menús guardados, así que se desactivó en lugar de eliminarse.",
            null => null
        };
        return RedirectToPage("./Index");
    }
}
