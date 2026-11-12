using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin.Platos;

public class CreateModel(PlatoService platoService) : PageModel
{
    [BindProperty]
    public CreateFormModel Plato { get; set; } = new();

    public bool Created { get; set; }
    public string? NuevoPlato { get; set; }
    public int PlatoId { get; set; }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // Verificar duplicado en la misma categoría
        var existe = await platoService.Query()
            .AnyAsync(p => p.Categoria == Plato.Categoria &&
                p.Nombre.Equals(Plato.Nombre, StringComparison.OrdinalIgnoreCase) &&
                p.Activo);

        if (existe)
        {
            ModelState.AddModelError("", $"Ya existe un plato '{Plato.Nombre}' en esa categoría.");
            return Page();
        }

        var plato = await platoService.CreateAsync(Plato.Nombre, Plato.Categoria);
        
        Created = true;
        NuevoPlato = plato.Nombre;
        PlatoId = plato.Id;

        return Page();
    }

    public class CreateFormModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Mínimo 2 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona una categoría")]
        public PlatoCategoria Categoria { get; set; }
    }
}
