using System.ComponentModel.DataAnnotations;

namespace MenuSemanal.Models;

public class Plato
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Franjas (combinables) en las que se puede servir el plato.</summary>
    [Required]
    public PlatoCategoria Franjas { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;

    public bool TieneFranja(PlatoCategoria franja) => (Franjas & franja) == franja;
}
