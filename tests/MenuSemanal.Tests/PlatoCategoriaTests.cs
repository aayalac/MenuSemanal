using MenuSemanal.Models;

namespace MenuSemanal.Tests;

public class PlatoCategoriaTests
{
    [Fact]
    public void Separar_DevuelveCadaFranjaEnOrden()
    {
        var franjas = (PlatoCategoria.Cena | PlatoCategoria.Desayuno).Separar();
        Assert.Equal([PlatoCategoria.Desayuno, PlatoCategoria.Cena], franjas);
    }

    [Fact]
    public void Separar_DeNinguna_EsVacio() =>
        Assert.Empty(PlatoCategoria.Ninguna.Separar());

    [Fact]
    public void Combinar_UneFranjasYToleraRepetidas()
    {
        var combinado = new[] { PlatoCategoria.Almuerzo, PlatoCategoria.Cena, PlatoCategoria.Cena }.Combinar();
        Assert.Equal(PlatoCategoria.Almuerzo | PlatoCategoria.Cena, combinado);
    }

    [Theory]
    [InlineData(PlatoCategoria.Almuerzo, true)]
    [InlineData(PlatoCategoria.Cena, true)]
    [InlineData(PlatoCategoria.Desayuno, false)]
    public void TieneFranja(PlatoCategoria franja, bool esperado)
    {
        var sopa = new Plato { Nombre = "Sopa", Franjas = PlatoCategoria.Almuerzo | PlatoCategoria.Cena };
        Assert.Equal(esperado, sopa.TieneFranja(franja));
    }
}
