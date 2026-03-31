using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data;

public class RestaurantDbContext : DbContext
{
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Waiter> Waiters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureDishesTable(modelBuilder);
        ConfigureRestaurantsTable(modelBuilder);
        ConfigureDishAvailabilityRelationship(modelBuilder);
        ConfigureWaitersTable(modelBuilder);
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

    private static void ConfigureRestaurantsTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Restaurant>().ToTable("Restaurants");
        modelBuilder.Entity<Restaurant>().HasKey(r => r.Id);
        modelBuilder.Entity<Restaurant>().Property(r => r.City).IsRequired();
        modelBuilder.Entity<Restaurant>().Property(r => r.Address).IsRequired();
        modelBuilder.Entity<Restaurant>().Property(r => r.City).HasMaxLength(100);
        modelBuilder.Entity<Restaurant>().Property(r => r.Address).HasMaxLength(200);
    }

    private static void ConfigureDishAvailabilityRelationship(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dish>()
            .HasMany(d => d.AvailableAtRestaurants)
            .WithMany(r => r.AvailableDishes)
            .UsingEntity("DishAvailability");
    }

    private static void ConfigureWaitersTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Waiter>().ToTable("Waiters");
        modelBuilder.Entity<Waiter>().HasKey(w => w.UserId);
        modelBuilder.Entity<Waiter>().Property(w => w.UserId).HasMaxLength(200);
        modelBuilder.Entity<Waiter>()
            .HasOne(w => w.Restaurant)
            .WithMany()
            .HasForeignKey(w => w.RestaurantId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}