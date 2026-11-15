using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin.Platos;

public class EditModel(PlatoService platoService) : PageModel
{
    [BindProperty]
    public PlatoForm Plato { get; set; } = new();

    public bool Encontrado { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var plato = await platoService.GetByIdAsync(id);
        if (plato == null) return Page();

        Encontrado = true;
        Plato = new PlatoForm
        {
            Id = plato.Id,
            Nombre = plato.Nombre,
            Franjas = plato.Franjas.Separar().ToList(),
            Activo = plato.Activo
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Encontrado = true;
        Plato.Id = id;

        if (!ModelState.IsValid)
            return Page();

        if (await platoService.ExisteNombreAsync(Plato.Nombre, excluirId: id))
        {
            ModelState.AddModelError("Plato.Nombre", $"Ya existe otro plato llamado '{Plato.Nombre.Trim()}'.");
            return Page();
        }

        if (!await platoService.UpdateAsync(id, Plato.Nombre, Plato.Franjas.Combinar(), Plato.Activo))
            return NotFound();

        TempData["SuccessMsg"] = $"Plato '{Plato.Nombre.Trim()}' actualizado correctamente.";
        return RedirectToPage("./Index");
    }
}
