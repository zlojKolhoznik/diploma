using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data;

public class RestaurantDbContext : DbContext
{
    public RestaurantDbContext()
    {
    }

    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
    {
    }

    public DbSet<Dish> Dishes { get; set; }
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Waiter> Waiters { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<WaiterSchedule> WaiterSchedules { get; set; }
    public DbSet<AdminAssignment> AdminAssignments { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<RestaurantWithAi.Core.Entities.UserProfile> UserProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureDishesTable(modelBuilder);
        ConfigureRestaurantsTable(modelBuilder);
        ConfigureDishAvailabilityRelationship(modelBuilder);
        ConfigureWaitersTable(modelBuilder);
        ConfigureWaiterSchedulesTable(modelBuilder);
        ConfigureAdminAssignmentsTable(modelBuilder);
        ConfigureTablesTable(modelBuilder);
        ConfigureReservationsTable(modelBuilder);
        ConfigureOrdersTable(modelBuilder);
        ConfigureOrderItemsTable(modelBuilder);
        ConfigureReviewsTable(modelBuilder);
        ConfigureReportsTable(modelBuilder);
        ConfigureUserProfilesTable(modelBuilder);
    }

    private static void ConfigureReviewsTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>().ToTable("Reviews");
        modelBuilder.Entity<Review>().HasKey(r => r.Id);
        modelBuilder.Entity<Review>().Property(r => r.CuisineRating).IsRequired();
        modelBuilder.Entity<Review>().Property(r => r.ServiceRating).IsRequired();
        modelBuilder.Entity<Review>().Property(r => r.CuisineComment).HasMaxLength(1000);
        modelBuilder.Entity<Review>().Property(r => r.ServiceComment).HasMaxLength(1000);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Reservation)
            .WithMany(r => r.Reviews)
            .HasForeignKey(r => r.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
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
        modelBuilder.Entity<Restaurant>().Property(r => r.AverageRating).HasColumnType("decimal(3,1)").IsRequired(false);
        modelBuilder.Entity<Restaurant>().Property(r => r.ImageUrl).HasMaxLength(2048).IsRequired(false);
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
        modelBuilder.Entity<Waiter>().Property(w => w.AverageRating).HasColumnType("decimal(3,1)").IsRequired(false);
        modelBuilder.Entity<Waiter>()
            .HasOne(w => w.Restaurant)
            .WithMany()
            .HasForeignKey(w => w.RestaurantId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }

    private static void ConfigureWaiterSchedulesTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WaiterSchedule>().ToTable("WaiterSchedules");
        modelBuilder.Entity<WaiterSchedule>().HasKey(ws => ws.Id);
        modelBuilder.Entity<WaiterSchedule>().Property(ws => ws.WaiterId).HasMaxLength(200);
        modelBuilder.Entity<WaiterSchedule>()
            .HasOne(ws => ws.Waiter)
            .WithMany(w => w.Schedules)
            .HasForeignKey(ws => ws.WaiterId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<WaiterSchedule>()
            .HasIndex(ws => new { ws.WaiterId, ws.Date })
            .IsUnique();
    }

    private static void ConfigureAdminAssignmentsTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminAssignment>().ToTable("AdminAssignments");
        modelBuilder.Entity<AdminAssignment>().HasKey(aa => aa.Id);
        modelBuilder.Entity<AdminAssignment>().Property(aa => aa.AppointedById).HasMaxLength(200);
        modelBuilder.Entity<AdminAssignment>().Property(aa => aa.AppointedUserId).HasMaxLength(200);

        // Relationship: who appointed this admin
        modelBuilder.Entity<AdminAssignment>()
            .HasOne(aa => aa.AppointedBy)
            .WithMany(w => w.AdminsAppointed)
            .HasForeignKey(aa => aa.AppointedById)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship: the appointed admin
        modelBuilder.Entity<AdminAssignment>()
            .HasOne(aa => aa.AppointedUser)
            .WithMany(w => w.AppointedBy)
            .HasForeignKey(aa => aa.AppointedUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create index for efficient lookups
        modelBuilder.Entity<AdminAssignment>()
            .HasIndex(aa => aa.AppointedById);
        modelBuilder.Entity<AdminAssignment>()
            .HasIndex(aa => aa.AppointedUserId);
    }

    private static void ConfigureTablesTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Table>().ToTable("Tables");
        modelBuilder.Entity<Table>().HasKey(t => new { t.RestaurantId, t.TableNumber });
        modelBuilder.Entity<Table>()
            .HasOne(t => t.Restaurant)
            .WithMany(r => r.Tables)
            .HasForeignKey(t => t.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureReservationsTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reservation>().ToTable("Reservations");
        modelBuilder.Entity<Reservation>().HasKey(r => r.Id);
        modelBuilder.Entity<Reservation>().Property(r => r.GuestId).HasMaxLength(200);
        modelBuilder.Entity<Reservation>().Property(r => r.GuestName).HasMaxLength(200);
        modelBuilder.Entity<Reservation>().Property(r => r.Status).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<Reservation>().Property(r => r.AssignedWaiterId).HasMaxLength(200);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Restaurant)
            .WithMany()
            .HasForeignKey(r => r.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Table)
            .WithMany()
            .HasForeignKey(r => new { r.RestaurantId, r.TableNumber })
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => new { r.RestaurantId, r.TableNumber, r.StartTime });
    }

    private static void ConfigureOrdersTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().ToTable("Orders");
        modelBuilder.Entity<Order>().HasKey(o => o.Id);
        modelBuilder.Entity<Order>().Property(o => o.Status).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<Order>().Property(o => o.Notes).HasMaxLength(500);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Reservation)
            .WithMany(r => r.Orders)
            .HasForeignKey(o => o.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Restaurant)
            .WithMany()
            .HasForeignKey(o => o.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.RestaurantId, o.ReservationId, o.Status });
    }

    private static void ConfigureOrderItemsTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
        modelBuilder.Entity<OrderItem>().HasKey(i => i.Id);
        modelBuilder.Entity<OrderItem>().Property(i => i.DishName).IsRequired().HasMaxLength(150);
        modelBuilder.Entity<OrderItem>().Property(i => i.Notes).HasMaxLength(300);
        modelBuilder.Entity<OrderItem>().Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<OrderItem>().Property(i => i.Status).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<OrderItem>().Property(i => i.RejectionReason).HasMaxLength(500).IsRequired(false);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Dish)
            .WithMany(d => d.OrderItems)
            .HasForeignKey(i => i.DishId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureReportsTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>().ToTable("Reports");
        modelBuilder.Entity<Report>().HasKey(r => r.Id);
        modelBuilder.Entity<Report>().Property(r => r.Type).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Report>().Property(r => r.Format).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<Report>().Property(r => r.GeneratedById).HasMaxLength(200);
        modelBuilder.Entity<Report>().Property(r => r.StorageKey).HasMaxLength(500);
    }

    // Configure user profiles table to store S3 storage key per user (no PII)
    private static void ConfigureUserProfilesTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RestaurantWithAi.Core.Entities.UserProfile>().ToTable("UserProfiles");
        modelBuilder.Entity<RestaurantWithAi.Core.Entities.UserProfile>().HasKey(u => u.UserId);
        modelBuilder.Entity<RestaurantWithAi.Core.Entities.UserProfile>().Property(u => u.UserId).HasMaxLength(200);
        modelBuilder.Entity<RestaurantWithAi.Core.Entities.UserProfile>().Property(u => u.PhotoStorageKey).HasMaxLength(1000).IsRequired(false);
    }
}