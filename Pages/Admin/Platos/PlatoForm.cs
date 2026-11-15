using System.ComponentModel.DataAnnotations;
using MenuSemanal.Models;

namespace MenuSemanal.Pages.Admin.Platos;

/// <summary>Datos del formulario de alta/edición de un plato.</summary>
public class PlatoForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Entre 2 y 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "Selecciona al menos una franja")]
    public List<PlatoCategoria> Franjas { get; set; } = [];

    public bool Activo { get; set; } = true;
}
