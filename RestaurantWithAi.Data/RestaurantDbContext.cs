using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data;

public class RestaurantDbContext : DbContext
{
    public DbSet<Dish> Dishes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureDishesTable(modelBuilder);
    }

    private static void ConfigureDishesTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dish>().ToTable("Dishes");
        modelBuilder.Entity<Dish>().HasKey(d => d.Id);
        modelBuilder.Entity<Dish>().Property(d => d.Name).IsRequired();
        modelBuilder.Entity<Dish>().Property(d => d.Description).IsRequired();
        modelBuilder.Entity<Dish>().Property(d => d.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Dish>().Property(d => d.ImageUrl).IsRequired();
        modelBuilder.Entity<Dish>().Property(d => d.Name).HasMaxLength(100);
        modelBuilder.Entity<Dish>().Property(d => d.Description).HasMaxLength(500);
    }
}