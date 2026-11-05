using Microsoft.EntityFrameworkCore;
using MenuSemanal.Data;
using MenuSemanal.Data.Seeding;
using MenuSemanal.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Base de datos SQLite ---
var dbPath = Environment.GetEnvironmentVariable("DB_PATH")
    ?? Path.Combine(builder.Environment.ContentRootPath, "Data", "menus.db");
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlite($"Data Source={dbPath}"));

// --- Servicios ---
builder.Services.AddScoped<PlatoService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddRazorPages();

var app = builder.Build();

// --- Aplicar migraciones automáticas y seed en desarrollo ---
if (!app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    ctx.Database.Migrate();
    await DbSeeder.SeedAsync(app.Services);
}

// --- Pipeline HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Health check endpoint for Docker HEALTHCHECK
app.MapGet("/health", () => Results.Ok("healthy"));

app.MapRazorPages();
app.Run();
