using System.Text.Json;
using MenuSemanal.Models;

namespace MenuSemanal.Services;

public class MenuService
{
    private readonly string _jsonPath;
    private MenuData? _menuData;

    public MenuService(IWebHostEnvironment env)
    {
        _jsonPath = Path.Combine(env.ContentRootPath, "Data", "comidas.json");
    }

    public async Task<MenuData> LoadMenuDataAsync()
    {
        if (_menuData != null)
            return _menuData;

        var json = await File.ReadAllTextAsync(_jsonPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _menuData = JsonSerializer.Deserialize<MenuData>(json, options) ?? new MenuData();
        return _menuData;
    }

    public async Task<List<DiaMenu>> GenerateWeeklyMenuAsync()
    {
        var data = await LoadMenuDataAsync();
        var rng = new Random();
        var dias = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

        var menu = new List<DiaMenu>();
        foreach (var dia in dias)
        {
            menu.Add(new DiaMenu
            {
                Dia = dia,
                Desayuno = data.Desayunos[rng.Next(data.Desayunos.Count)].Nombre,
                Almuerzo = data.Almuerzos[rng.Next(data.Almuerzos.Count)].Nombre,
                Cena = data.Cenas[rng.Next(data.Cenas.Count)].Nombre
            });
        }

        return menu;
    }

    public async Task<string> GetRandomDesayunoAsync()
    {
        var data = await LoadMenuDataAsync();
        return data.Desayunos[Random.Shared.Next(data.Desayunos.Count)].Nombre;
    }

    public async Task<string> GetRandomAlmuerzoAsync()
    {
        var data = await LoadMenuDataAsync();
        return data.Almuerzos[Random.Shared.Next(data.Almuerzos.Count)].Nombre;
    }

    public async Task<string> GetRandomCenaAsync()
    {
        var data = await LoadMenuDataAsync();
        return data.Cenas[Random.Shared.Next(data.Cenas.Count)].Nombre;
    }
}
