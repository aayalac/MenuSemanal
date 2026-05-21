namespace MenuSemanal.Models;

public class MenuData
{
    public List<ComidaItem> Desayunos { get; set; } = new();
    public List<ComidaItem> Almuerzos { get; set; } = new();
    public List<ComidaItem> Cenas { get; set; } = new();
}
