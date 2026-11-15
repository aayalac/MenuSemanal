using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin.Platos;

public class CreateModel(PlatoService platoService) : PageModel
{
    [BindProperty]
    public PlatoForm Plato { get; set; } = new();

    public bool Created { get; set; }
    public string? NuevoPlato { get; set; }
    public int PlatoId { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (await platoService.ExisteNombreAsync(Plato.Nombre))
        {
            ModelState.AddModelError("Plato.Nombre",
                $"Ya existe un plato '{Plato.Nombre.Trim()}'. Edítalo para agregarle más franjas.");
            return Page();
        }

        var plato = await platoService.CreateAsync(Plato.Nombre, Plato.Franjas.Combinar());

        Created = true;
        NuevoPlato = plato.Nombre;
        PlatoId = plato.Id;

        ModelState.Clear();
        Plato = new PlatoForm();
        return Page();
    }
}
