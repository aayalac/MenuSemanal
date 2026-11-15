using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin.Platos;

public record EstadisticasFranjas(int Desayuno, int Almuerzo, int Cena);

public class IndexModel(PlatoService platoService) : PageModel
{
    public List<Plato> Platos { get; set; } = [];
    public EstadisticasFranjas Estadisticas { get; set; } = new EstadisticasFranjas(0, 0, 0);
    public int Total { get; set; }
    public string? Filtro { get; set; }

    public async Task OnGetAsync([FromQuery] string? categoria = null)
    {
        var all = await platoService.GetAllAsync();
        Total = all.Count;
        Estadisticas = new EstadisticasFranjas(
            all.Count(p => p.TieneFranja(PlatoCategoria.Desayuno)),
            all.Count(p => p.TieneFranja(PlatoCategoria.Almuerzo)),
            all.Count(p => p.TieneFranja(PlatoCategoria.Cena))
        );

        if (Enum.TryParse(categoria, out PlatoCategoria franja) && franja != PlatoCategoria.Ninguna)
        {
            Filtro = franja.ToString();
            Platos = all.Where(p => p.TieneFranja(franja)).ToList();
        }
        else
        {
            Platos = all;
        }
    }
}
