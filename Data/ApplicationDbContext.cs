using Microsoft.EntityFrameworkCore;
using MenuSemanal.Models;

namespace MenuSemanal.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Plato> Platoss => Set<Plato>();
    public DbSet<MenuSemanario> MenuSemanars => Set<MenuSemanario>();
    public DbSet<MenuDia> MenuDias => Set<MenuDia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de entidades
        modelBuilder.Entity<Plato>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Nombre);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
        });

        modelBuilder.Entity<MenuSemanario>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Índice NO único: se pueden guardar varios menús para la misma semana
            // (cada vez que se pulsa "Generar Nuevo Menú" queda registrado en el historial).
            entity.HasIndex(e => e.SemanaFecha);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
        });

        modelBuilder.Entity<MenuDia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(md => md.MenuSemanarioRef!)
                .WithMany(ms => ms.Dias)
                .HasForeignKey(md => md.MenuSemanalId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(md => md.Desayuno)
                .WithMany()
                .HasForeignKey(md => md.DesayunoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(md => md.Almuerzo)
                .WithMany()
                .HasForeignKey(md => md.AlmuerzoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(md => md.Cena)
                .WithMany()
                .HasForeignKey(md => md.CenaId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
