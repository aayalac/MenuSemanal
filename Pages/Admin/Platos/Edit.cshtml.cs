using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin.Platos;

public class EditModel(PlatoService platoService) : PageModel
{
    [BindProperty]
    public PlatoDto Plato { get; set; } = new();

    public Plato? PlatoOriginal { get; set; }
    public string? Message { get; set; }
    public bool Success { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var plato = await platoService.GetByIdAsync(id);
        if (plato == null)
        {
            Message = "El plato solicitado no fue encontrado.";
            return Page();
        }

        PlatoOriginal = plato;
        Plato = new PlatoDto
        {
            Id = plato.Id,
            Nombre = plato.Nombre,
            Categoria = plato.Categoria,
            Activo = plato.Activo
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid && PlatoOriginal != null)
        {
            var ok = await platoService.UpdateAsync(Plato.Id, Plato.Nombre, Plato.Categoria);
            if (ok)
            {
                Message = "Plato actualizado correctamente.";
                Success = true;
                return RedirectToPage("./Index");
            }
            Message = "Error al actualizar el plato.";
            Success = false;
        }
        return Page();
    }

    public class PlatoDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200, ErrorMessage = "Máximo 200 caracteres")]
        public string Nombre { get; set; } = "";
        [Required]
        public PlatoCategoria Categoria { get; set; }
        public bool Activo { get; set; } = true;
    }
}
