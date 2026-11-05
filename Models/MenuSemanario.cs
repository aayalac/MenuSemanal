using System.ComponentModel.DataAnnotations;

namespace MenuSemanal.Models;

public class MenuSemanario
{
    public int Id { get; set; }

    [Required]
    public DateOnly SemanaFecha { get; set; }

    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<MenuDia> Dias { get; set; } = new List<MenuDia>();
}
