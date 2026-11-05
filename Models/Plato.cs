using System.ComponentModel.DataAnnotations;

namespace MenuSemanal.Models;

public class Plato
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public PlatoCategoria Categoria { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
}
