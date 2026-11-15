using Microsoft.EntityFrameworkCore;
using MenuSemanal.Data;
using MenuSemanal.Models;

namespace MenuSemanal.Services;

/// <summary>
/// Servicio CRUD para administración de platos/catálogo de comidas.
/// </summary>
public class PlatoService(ApplicationDbContext dbContext)
{
    public Task<List<Plato>> GetAllAsync() =>
        dbContext.Platos.OrderBy(p => p.Nombre).ToListAsync();

    /// <summary>Platos activos que pueden servirse en la franja indicada.</summary>
    public Task<List<Plato>> GetActivosPorFranjaAsync(PlatoCategoria franja) =>
        dbContext.Platos
            .Where(p => p.Activo && (p.Franjas & franja) == franja)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

    public Task<Plato?> GetByIdAsync(int id) =>
        dbContext.Platos.FindAsync(id).AsTask();

    /// <summary>
    /// ¿Existe otro plato con el mismo nombre (sin distinguir mayúsculas)?
    /// Los nombres son únicos: un mismo plato se reutiliza en varias franjas.
    /// </summary>
    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null)
    {
        var normalizado = nombre.Trim().ToLower();
        return dbContext.Platos.AnyAsync(p =>
            p.Nombre.ToLower() == normalizado && (excluirId == null || p.Id != excluirId));
    }

    public async Task<Plato> CreateAsync(string nombre, PlatoCategoria franjas)
    {
        var plato = new Plato
        {
            Nombre = nombre.Trim(),
            Franjas = franjas
        };
        dbContext.Platos.Add(plato);
        await dbContext.SaveChangesAsync();
        return plato;
    }

    public async Task<bool> UpdateAsync(int id, string nombre, PlatoCategoria franjas, bool activo)
    {
        var plato = await dbContext.Platos.FindAsync(id);
        if (plato == null) return false;

        plato.Nombre = nombre.Trim();
        plato.Franjas = franjas;
        plato.Activo = activo;
        await dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>¿El plato aparece en algún menú guardado?</summary>
    public Task<bool> EstaEnUsoAsync(int id) =>
        dbContext.MenuDias.AnyAsync(d => d.DesayunoId == id || d.AlmuerzoId == id || d.CenaId == id);

    /// <summary>
    /// Elimina el plato. Si ya se usó en algún menú solo se desactiva,
    /// para no dejar huecos en el historial.
    /// </summary>
    /// <returns>null si no existe; true si se borró; false si solo se desactivó.</returns>
    public async Task<bool?> DeleteOrDeactivateAsync(int id)
    {
        var plato = await dbContext.Platos.FindAsync(id);
        if (plato == null) return null;

        if (await EstaEnUsoAsync(id))
        {
            plato.Activo = false;
            await dbContext.SaveChangesAsync();
            return false;
        }

        dbContext.Platos.Remove(plato);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
