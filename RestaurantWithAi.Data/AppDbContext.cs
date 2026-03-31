using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Data.Entities;

namespace RestaurantWithAi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Waiter> Waiters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Waiter>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.UserId).IsRequired();
            entity.HasIndex(w => w.UserId).IsUnique();
        });
    }
}
