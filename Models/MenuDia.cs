using System.ComponentModel.DataAnnotations;

namespace MenuSemanal.Models;

public class MenuDia
{
    public int Id { get; set; }

    public int MenuSemanalId { get; set; }

    /// <summary>
    /// Día de la semana (1=Lunes, ..., 7=Domingo)
    /// </summary>
    [Range(1, 7)]
    public int DiaSemana { get; set; }

    public string DiaNombre => DiaSemana switch
    {
        1 => "Lunes",
        2 => "Martes",
        3 => "Miércoles",
        4 => "Jueves",
        5 => "Viernes",
        6 => "Sábado",
        7 => "Domingo",
        _ => throw new ArgumentOutOfRangeException(nameof(DiaSemana))
    };

    /// <summary>
    /// Relación con el desayuno asignado
    /// </summary>
    public int? DesayunoId { get; set; }
    public MenuSemanario? MenuSemanarioRef { get; set; }
    public Plato? Desayuno { get; set; }

    /// <summary>
    /// Relación con el almuerzo asignado
    /// </summary>
    public int? AlmuerzoId { get; set; }
    public Plato? Almuerzo { get; set; }

    /// <summary>
    /// Relación con la cena asignada
    /// </summary>
    public int? CenaId { get; set; }
    public Plato? Cena { get; set; }
}
