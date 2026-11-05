using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages.Admin.Platos;

public record Estats(int Desayuno, int Almuerzo, int Cena);

public class IndexModel(PlatoService platoService) : PageModel
{
    public List<Plato> Platoss { get; set; } = [];
    public Estats Estads { get; set; } = new Estats(0, 0, 0);
    public string? Filtro { get; set; }

    public async Task OnGetAsync([FromQuery] string? categoria = null)
    {
        Filtro = categoria;
        var all = await platoService.GetAllAsync();
        Estads = new Estats(
            all.Count(p => p.Categoria == PlatoCategoria.Desayuno),
            all.Count(p => p.Categoria == PlatoCategoria.Almuerzo),
            all.Count(p => p.Categoria == PlatoCategoria.Cena)
        );

        Platoss = !string.IsNullOrWhiteSpace(categoria) && Enum.TryParse(categoria, out PlatoCategoria cat)
            ? all.Where(p => p.Categoria == cat).OrderBy(p => p.Nombre).ToList()
            : all.OrderBy(p => p.Categoria).ThenBy(p => p.Nombre).ToList();
    }
}
