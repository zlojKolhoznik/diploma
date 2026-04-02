using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Data;
using RestaurantWithAi.Data.Repositories;

namespace RestaurantWithAi.Tests.Data;

[ExcludeFromCodeCoverage]
public class ReservationRepositoryTests
{
    [Fact]
    public async Task HasTableConflictAsync_WhenGapIsLessThan15Minutes_ReturnsTrue()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        context.Tables.Add(new Table { RestaurantId = restaurant.Id, TableNumber = 1, Seats = 4 });

        context.Reservations.Add(new Reservation
        {
            RestaurantId = restaurant.Id,
            TableNumber = 1,
            GuestId = "guest-1",
            StartTime = new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc),
            ApproximateDurationMinutes = 60,
            NumberOfGuests = 2,
            Status = ReservationStatuses.Created
        });
        await context.SaveChangesAsync();

        var sut = new ReservationRepository(context);

        var hasConflict = await sut.HasTableConflictAsync(
            restaurant.Id,
            1,
            new DateTime(2026, 4, 2, 13, 10, 0, DateTimeKind.Utc),
            30);

        Assert.True(hasConflict);
    }

    [Fact]
    public async Task GetAvailableTablesAsync_WhenTableConflicts_ExcludesItFromResult()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        context.Tables.AddRange(
            new Table { RestaurantId = restaurant.Id, TableNumber = 1, Seats = 4 },
            new Table { RestaurantId = restaurant.Id, TableNumber = 2, Seats = 4 });

        context.Reservations.Add(new Reservation
        {
            RestaurantId = restaurant.Id,
            TableNumber = 1,
            GuestId = "guest-1",
            StartTime = new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc),
            ApproximateDurationMinutes = 60,
            NumberOfGuests = 2,
            Status = ReservationStatuses.Created
        });
        await context.SaveChangesAsync();

        var sut = new ReservationRepository(context);

        var available = (await sut.GetAvailableTablesAsync(
            restaurant.Id,
            new DateTime(2026, 4, 2, 12, 30, 0, DateTimeKind.Utc),
            30)).ToList();

        Assert.Single(available);
        Assert.Equal(2, available[0].TableNumber);
    }

    private static Restaurant CreateRestaurant()
    {
        return new Restaurant
        {
            Id = Guid.NewGuid(),
            City = "Kyiv",
            Address = "Address 1"
        };
    }

    private static RestaurantDbContext CreateContext()
    {
        return new TestRestaurantDbContext(Guid.NewGuid().ToString("N"));
    }

    private sealed class TestRestaurantDbContext(string databaseName) : RestaurantDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName);
        }
    }
}

