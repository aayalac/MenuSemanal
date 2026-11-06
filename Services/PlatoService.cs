using Microsoft.EntityFrameworkCore;
using MenuSemanal.Data;
using MenuSemanal.Models;

namespace MenuSemanal.Services;

/// <summary>
/// Servicio CRUD para administración de platos/catálogo de comidas.
/// </summary>
public class PlatoService(ApplicationDbContext dbContext)
{
    public IQueryable<Plato> Query() => dbContext.Platoss.AsQueryable();

    public Task<List<Plato>> GetAllAsync() => dbContext.Platoss.ToListAsync();

    public Task<List<Plato>> GetByCategoryAsync(PlatoCategoria categoria) =>
        dbContext.Platoss.Where(p => p.Categoria == categoria && p.Activo).ToListAsync();

    public Task<Plato?> GetByIdAsync(int id) =>
        dbContext.Platoss.FindAsync(id).AsTask();

    public async Task<Plato> CreateAsync(string nombre, PlatoCategoria categoria)
    {
        var plato = new Plato
        {
            Nombre = nombre.Trim(),
            Categoria = categoria
        };
        dbContext.Platoss.Add(plato);
        await dbContext.SaveChangesAsync();
        return plato;
    }

    public async Task<bool> UpdateAsync(int id, string nombre, PlatoCategoria categoria)
    {
        var plato = await dbContext.Platoss.FindAsync(id);
        if (plato == null) return false;

        plato.Nombre = nombre.Trim();
        plato.Categoria = categoria;
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var plato = await dbContext.Platoss.FindAsync(id);
        if (plato == null) return false;

        dbContext.Platoss.Remove(plato);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var plato = await dbContext.Platoss.FindAsync(id);
        if (plato == null) return false;

        plato.Activo = false;
        await dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Obtiene nombres únicos activos por categoría, ideal para dropdowns.
    /// </summary>
    public async Task<(List<string> desayunos, List<string> almuerzos, List<string> cenas)> GetActiveNamesByCategoryAsync()
    {
        var q = dbContext.Platoss.Where(p => p.Activo);
        return (
            await q.Where(p => p.Categoria == PlatoCategoria.Desayuno).Select(p => p.Nombre).OrderBy(n => n).ToListAsync(),
            await q.Where(p => p.Categoria == PlatoCategoria.Almuerzo).Select(p => p.Nombre).OrderBy(n => n).ToListAsync(),
            await q.Where(p => p.Categoria == PlatoCategoria.Cena).Select(p => p.Nombre).OrderBy(n => n).ToListAsync()
        );
    }
}
