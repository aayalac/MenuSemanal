namespace MenuSemanal.Models;

/// <summary>
/// Franjas horarias en las que se puede servir un plato.
/// Es un enum de banderas: un plato puede pertenecer a varias franjas
/// a la vez (ej.: Sopa → Almuerzo | Cena).
/// </summary>
[Flags]
public enum PlatoCategoria
{
    Ninguna = 0,
    Desayuno = 1,
    Almuerzo = 2,
    Cena = 4
}

public static class PlatoCategoriaExtensions
{
    /// <summary>Franjas individuales, en orden del día.</summary>
    public static readonly PlatoCategoria[] Todas =
        [PlatoCategoria.Desayuno, PlatoCategoria.Almuerzo, PlatoCategoria.Cena];

    /// <summary>Descompone un valor combinado en sus franjas individuales.</summary>
    public static IEnumerable<PlatoCategoria> Separar(this PlatoCategoria franjas) =>
        Todas.Where(f => (franjas & f) == f);

    public static PlatoCategoria Combinar(this IEnumerable<PlatoCategoria> franjas) =>
        franjas.Aggregate(PlatoCategoria.Ninguna, (acc, f) => acc | f);
}
