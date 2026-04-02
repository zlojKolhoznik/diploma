using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Data;
using RestaurantWithAi.Data.Repositories;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Tests.Data;

[ExcludeFromCodeCoverage]
public class TableRepositoryTests
{
    #region GetTablesByRestaurantIdAsync

    [Fact]
    public async Task GetTablesByRestaurantIdAsync_WhenRestaurantExists_ReturnsTablesForRestaurant()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        context.Tables.AddRange(
            CreateTable(1, 4, restaurant.Id),
            CreateTable(2, 2, restaurant.Id));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetTablesByRestaurantIdAsync(restaurant.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(restaurant.Id, t.RestaurantId));
    }

    [Fact]
    public async Task GetTablesByRestaurantIdAsync_WhenNoTablesExist_ReturnsEmptyCollection()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = await sut.GetTablesByRestaurantIdAsync(restaurant.Id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTablesByRestaurantIdAsync_WhenRestaurantDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetTablesByRestaurantIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetTablesByRestaurantIdAsync_OnlyReturnsTablesForSpecifiedRestaurant()
    {
        await using var context = CreateContext();
        var restaurant1 = CreateRestaurant("Kyiv", "Address 1");
        var restaurant2 = CreateRestaurant("Lviv", "Address 2");
        context.Restaurants.AddRange(restaurant1, restaurant2);
        context.Tables.AddRange(
            CreateTable(1, 4, restaurant1.Id),
            CreateTable(1, 6, restaurant2.Id));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetTablesByRestaurantIdAsync(restaurant1.Id)).ToList();

        Assert.Single(result);
        Assert.Equal(restaurant1.Id, result[0].RestaurantId);
    }

    [Fact]
    public async Task GetTablesByRestaurantIdAsync_WhenCalled_DoesNotTrackEntities()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        context.Tables.Add(CreateTable(1, 4, restaurant.Id));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var sut = CreateSut(context);

        _ = await sut.GetTablesByRestaurantIdAsync(restaurant.Id);

        Assert.Empty(context.ChangeTracker.Entries<Table>());
    }

    #endregion

    #region AddTableAsync

    [Fact]
    public async Task AddTableAsync_WhenTableIsValid_PersistsTable()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);
        var table = CreateTable(5, 4, restaurant.Id);

        await sut.AddTableAsync(table);

        var persisted = await context.Tables.SingleAsync(t => t.RestaurantId == restaurant.Id && t.TableNumber == 5);
        Assert.Equal(4, persisted.Seats);
    }

    [Fact]
    public async Task AddTableAsync_WhenTableIsNull_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddTableAsync(null!));
    }

    [Fact]
    public async Task AddTableAsync_WhenRestaurantDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);
        var table = CreateTable(1, 4, Guid.NewGuid());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.AddTableAsync(table));
    }

    #endregion

    #region DeleteTableAsync

    [Fact]
    public async Task DeleteTableAsync_WhenTableExists_RemovesTable()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        context.Tables.Add(CreateTable(1, 4, restaurant.Id));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await sut.DeleteTableAsync(restaurant.Id, 1);

        var exists = await context.Tables.AnyAsync(t => t.RestaurantId == restaurant.Id && t.TableNumber == 1);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteTableAsync_WhenTableDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.DeleteTableAsync(restaurant.Id, 99));
    }

    #endregion

    #region UpdateTableSeatsAsync

    [Fact]
    public async Task UpdateTableSeatsAsync_WhenTableExists_UpdatesSeats()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        context.Tables.Add(CreateTable(1, 4, restaurant.Id));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await sut.UpdateTableSeatsAsync(restaurant.Id, 1, 8);

        var persisted = await context.Tables.SingleAsync(t => t.RestaurantId == restaurant.Id && t.TableNumber == 1);
        Assert.Equal(8, persisted.Seats);
    }

    [Fact]
    public async Task UpdateTableSeatsAsync_WhenTableDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateTableSeatsAsync(restaurant.Id, 99, 4));
    }

    #endregion

    #region GetAvailableTablesAsync

    [Fact]
    public async Task GetAvailableTablesAsync_WhenNoReservations_ReturnsAllTables()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        context.Tables.AddRange(
            CreateTable(1, 4, restaurant.Id),
            CreateTable(2, 2, restaurant.Id));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetAvailableTablesAsync(restaurant.Id, DateTime.UtcNow.AddHours(1), 60)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAvailableTablesAsync_WhenTableHasOverlappingReservation_ExcludesOccupiedTable()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        context.Tables.AddRange(
            CreateTable(1, 4, restaurant.Id),
            CreateTable(2, 2, restaurant.Id));
        var startTime = DateTime.UtcNow.AddHours(2);
        context.Reservations.Add(CreateReservation(restaurant.Id, 1, startTime, 60));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetAvailableTablesAsync(restaurant.Id, startTime, 60)).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].TableNumber);
    }

    [Fact]
    public async Task GetAvailableTablesAsync_WhenRestaurantDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.GetAvailableTablesAsync(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 60));
    }

    #endregion

    private static TableRepository CreateSut(RestaurantDbContext context)
    {
        return new TableRepository(context);
    }

    private static RestaurantDbContext CreateContext()
    {
        return new TestRestaurantDbContext(Guid.NewGuid().ToString("N"));
    }

    private static Table CreateTable(int tableNumber, int seats, Guid restaurantId)
    {
        return new Table { TableNumber = tableNumber, Seats = seats, RestaurantId = restaurantId };
    }

    private static Reservation CreateReservation(Guid restaurantId, int tableNumber, DateTime startTime, int durationMinutes)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            TableNumber = tableNumber,
            GuestName = "Test Guest",
            StartTime = startTime,
            DurationMinutes = durationMinutes,
            NumberOfGuests = 2,
            Status = ReservationStatus.Created
        };
    }

    private static Restaurant CreateRestaurant(string city, string address)
    {
        return new Restaurant
        {
            Id = Guid.NewGuid(),
            City = city,
            Address = address
        };
    }

    private sealed class TestRestaurantDbContext(string databaseName) : RestaurantDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName);
        }
    }
}
