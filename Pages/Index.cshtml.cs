using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MenuSemanal.Models;
using MenuSemanal.Services;

namespace MenuSemanal.Pages;

public class IndexModel(MenuService menuService, PlatoService platoService, ILogger<IndexModel> logger) : PageModel
{
    public MenuSemanario? Menu { get; private set; }

    /// <summary>Opciones de cada franja para los desplegables.</summary>
    public Dictionary<PlatoCategoria, List<Plato>> Opciones { get; } = [];

    [BindProperty]
    public int MenuId { get; set; }

    [BindProperty]
    public List<DiaForm> Dias { get; set; } = [];

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    /// <summary>Aviso de la propia petición GET (no se guarda en TempData).</summary>
    public string? Aviso { get; private set; }

    public async Task OnGetAsync()
    {
        Menu = await menuService.GetLatestMenuAsync();

        if (Menu == null)
        {
            // Primera visita: generar un menú automáticamente.
            try
            {
                Menu = await menuService.GenerateWeeklyMenuAsync();
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "No se pudo generar el menú inicial");
                Aviso = ex.Message + " Agrégalos desde Administración.";
            }
        }

        await CargarFormularioAsync();
    }

    public async Task<IActionResult> OnPostGenerarAsync()
    {
        try
        {
            await menuService.GenerateWeeklyMenuAsync();
            SuccessMessage = "✨ ¡Menú semanal generado y guardado en el historial!";
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Error al generar el menú semanal");
            ErrorMessage = "No se pudo generar el menú. Revisa que exista al menos un plato activo por franja.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGuardarAsync()
    {
        var cambios = Dias.Select(d => new CambioDia(d.DiaSemana, d.DesayunoId, d.AlmuerzoId, d.CenaId));
        if (await menuService.UpdateMenuAsync(MenuId, cambios))
            SuccessMessage = "💾 Cambios guardados.";
        else
            ErrorMessage = "No se pudieron guardar los cambios del menú.";

        return RedirectToPage();
    }

    private async Task CargarFormularioAsync()
    {
        if (Menu == null) return;

        MenuId = Menu.Id;
        Dias = Menu.Dias.OrderBy(d => d.DiaSemana).Select(d => new DiaForm
        {
            DiaSemana = d.DiaSemana,
            Nombre = d.DiaNombre,
            DesayunoId = d.DesayunoId,
            AlmuerzoId = d.AlmuerzoId,
            CenaId = d.CenaId
        }).ToList();

        foreach (var franja in PlatoCategoriaExtensions.Todas)
        {
            var activos = await platoService.GetActivosPorFranjaAsync(franja);

            // Conservar en la lista los platos ya asignados aunque estén inactivos.
            var asignados = Menu.Dias
                .Select(d => franja switch
                {
                    PlatoCategoria.Desayuno => d.Desayuno,
                    PlatoCategoria.Almuerzo => d.Almuerzo,
                    _ => d.Cena
                })
                .OfType<Plato>()
                .Where(p => activos.All(a => a.Id != p.Id));

            Opciones[franja] = activos.Concat(asignados.DistinctBy(p => p.Id)).OrderBy(p => p.Nombre).ToList();
        }
    }

    public int? SeleccionDe(DiaForm dia, PlatoCategoria franja) => franja switch
    {
        PlatoCategoria.Desayuno => dia.DesayunoId,
        PlatoCategoria.Almuerzo => dia.AlmuerzoId,
        _ => dia.CenaId
    };

    public class DiaForm
    {
        public int DiaSemana { get; set; }
        public string Nombre { get; set; } = "";
        public int? DesayunoId { get; set; }
        public int? AlmuerzoId { get; set; }
        public int? CenaId { get; set; }
    }
}
