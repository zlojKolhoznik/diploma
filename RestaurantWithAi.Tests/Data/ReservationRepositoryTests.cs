using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Data;
using RestaurantWithAi.Data.Repositories;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Tests.Data;

[ExcludeFromCodeCoverage]
public class ReservationRepositoryTests
{
    #region GetReservationsByGuestIdAsync

    [Fact]
    public async Task GetReservationsByGuestIdAsync_WhenGuestHasReservations_ReturnsThoseReservations()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        var guestId = "guest-abc";
        context.Reservations.AddRange(
            CreateReservation(restaurant.Id, guestId: guestId),
            CreateReservation(restaurant.Id, guestId: guestId),
            CreateReservation(restaurant.Id, guestId: "other-guest"));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetReservationsByGuestIdAsync(guestId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(guestId, r.GuestId));
    }

    [Fact]
    public async Task GetReservationsByGuestIdAsync_WhenGuestHasNoReservations_ReturnsEmpty()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        var result = await sut.GetReservationsByGuestIdAsync("nonexistent-guest");

        Assert.Empty(result);
    }

    #endregion

    #region GetReservationsByWaiterIdAsync

    [Fact]
    public async Task GetReservationsByWaiterIdAsync_WhenWaiterHasReservations_ReturnsThoseReservations()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        var waiterId = "waiter-xyz";
        context.Reservations.AddRange(
            CreateReservation(restaurant.Id, waiterId: waiterId),
            CreateReservation(restaurant.Id, waiterId: "other-waiter"));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetReservationsByWaiterIdAsync(waiterId)).ToList();

        Assert.Single(result);
        Assert.Equal(waiterId, result[0].WaiterId);
    }

    #endregion

    #region GetReservationByIdAsync

    [Fact]
    public async Task GetReservationByIdAsync_WhenReservationExists_ReturnsReservation()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        var reservation = CreateReservation(restaurant.Id);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = await sut.GetReservationByIdAsync(reservation.Id);

        Assert.Equal(reservation.Id, result.Id);
    }

    [Fact]
    public async Task GetReservationByIdAsync_WhenReservationDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetReservationByIdAsync(Guid.NewGuid()));
    }

    #endregion

    #region AddReservationAsync

    [Fact]
    public async Task AddReservationAsync_WhenReservationIsValid_PersistsReservation()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);
        var reservation = CreateReservation(restaurant.Id);

        var result = await sut.AddReservationAsync(reservation);

        Assert.NotNull(result);
        Assert.Equal(reservation.Id, result.Id);
        Assert.Equal(1, await context.Reservations.CountAsync());
    }

    [Fact]
    public async Task AddReservationAsync_WhenReservationIsNull_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddReservationAsync(null!));
    }

    #endregion

    #region HasOverlappingReservationAsync

    [Fact]
    public async Task HasOverlappingReservationAsync_WhenNoExistingReservations_ReturnsFalse()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = await sut.HasOverlappingReservationAsync(
            restaurant.Id, 1, DateTimeOffset.UtcNow.AddHours(2), 60);

        Assert.False(result);
    }

    [Fact]
    public async Task HasOverlappingReservationAsync_WhenReservationOverlaps_ReturnsTrue()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        var baseTime = DateTimeOffset.UtcNow.AddHours(2);
        context.Reservations.Add(CreateReservation(restaurant.Id, tableNumber: 1,
            startTime: baseTime, durationMinutes: 60));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        // Tries to book overlapping time on same table
        var result = await sut.HasOverlappingReservationAsync(
            restaurant.Id, 1, baseTime.AddMinutes(30), 60);

        Assert.True(result);
    }

    [Fact]
    public async Task HasOverlappingReservationAsync_WhenGapIsEnforced_ReturnsFalse()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        var baseTime = DateTimeOffset.UtcNow.AddHours(2);
        // Existing reservation: baseTime to baseTime + 60min
        context.Reservations.Add(CreateReservation(restaurant.Id, tableNumber: 1,
            startTime: baseTime, durationMinutes: 60));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        // New reservation starts 16 minutes after existing ends (>15 min gap required)
        var result = await sut.HasOverlappingReservationAsync(
            restaurant.Id, 1, baseTime.AddMinutes(76), 60);

        Assert.False(result);
    }

    [Fact]
    public async Task HasOverlappingReservationAsync_WhenGapIsInsufficientAfter_ReturnsTrue()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        var baseTime = DateTimeOffset.UtcNow.AddHours(2);
        // Existing reservation: baseTime to baseTime+60min
        context.Reservations.Add(CreateReservation(restaurant.Id, tableNumber: 1,
            startTime: baseTime, durationMinutes: 60));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        // New reservation starts only 10 minutes after existing ends (< 15 min gap)
        var result = await sut.HasOverlappingReservationAsync(
            restaurant.Id, 1, baseTime.AddMinutes(70), 60);

        Assert.True(result);
    }

    [Fact]
    public async Task HasOverlappingReservationAsync_WhenCancelledReservationExists_ReturnsFalse()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        var baseTime = DateTimeOffset.UtcNow.AddHours(2);
        context.Reservations.Add(CreateReservation(restaurant.Id, tableNumber: 1,
            startTime: baseTime, durationMinutes: 60, status: ReservationStatus.Cancelled));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = await sut.HasOverlappingReservationAsync(
            restaurant.Id, 1, baseTime, 60);

        Assert.False(result);
    }

    [Fact]
    public async Task HasOverlappingReservationAsync_WhenExcludingOwnReservation_ReturnsFalse()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        var baseTime = DateTimeOffset.UtcNow.AddHours(2);
        var reservation = CreateReservation(restaurant.Id, tableNumber: 1,
            startTime: baseTime, durationMinutes: 60);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        // Excluding itself - should return false
        var result = await sut.HasOverlappingReservationAsync(
            restaurant.Id, 1, baseTime, 60, reservation.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task HasOverlappingReservationAsync_WhenDifferentTable_ReturnsFalse()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        var baseTime = DateTimeOffset.UtcNow.AddHours(2);
        context.Reservations.Add(CreateReservation(restaurant.Id, tableNumber: 1,
            startTime: baseTime, durationMinutes: 60));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        // Table 2 should not be affected by table 1 reservation
        var result = await sut.HasOverlappingReservationAsync(
            restaurant.Id, 2, baseTime, 60);

        Assert.False(result);
    }

    #endregion

    #region GetAvailableTableNumbersAsync

    [Fact]
    public async Task GetAvailableTableNumbersAsync_WhenNoReservations_ReturnsAllTables()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        context.Tables.AddRange(
            CreateTable(1, restaurant.Id),
            CreateTable(2, restaurant.Id),
            CreateTable(3, restaurant.Id));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetAvailableTableNumbersAsync(
            restaurant.Id, DateTimeOffset.UtcNow.AddHours(2), 60)).ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAvailableTableNumbersAsync_WhenTableIsOccupied_ExcludesItFromResults()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        context.Tables.AddRange(
            CreateTable(1, restaurant.Id),
            CreateTable(2, restaurant.Id));
        var baseTime = DateTimeOffset.UtcNow.AddHours(2);
        context.Reservations.Add(CreateReservation(restaurant.Id, tableNumber: 1,
            startTime: baseTime, durationMinutes: 60));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetAvailableTableNumbersAsync(restaurant.Id, baseTime, 60)).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0]);
    }

    #endregion

    #region HasAvailableTablesAsync

    [Fact]
    public async Task HasAvailableTablesAsync_WhenNoTables_ReturnsFalse()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = await sut.HasAvailableTablesAsync(
            restaurant.Id, DateTimeOffset.UtcNow.AddHours(1), 60);

        Assert.False(result);
    }

    [Fact]
    public async Task HasAvailableTablesAsync_WhenTableIsAvailable_ReturnsTrue()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        context.Tables.Add(CreateTable(1, restaurant.Id));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = await sut.HasAvailableTablesAsync(
            restaurant.Id, DateTimeOffset.UtcNow.AddHours(2), 60);

        Assert.True(result);
    }

    [Fact]
    public async Task HasAvailableTablesAsync_WhenAllTablesOccupied_ReturnsFalse()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        context.Tables.Add(CreateTable(1, restaurant.Id));
        var baseTime = DateTimeOffset.UtcNow.AddHours(2);
        context.Reservations.Add(CreateReservation(restaurant.Id, tableNumber: 1,
            startTime: baseTime, durationMinutes: 60));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = await sut.HasAvailableTablesAsync(restaurant.Id, baseTime, 60);

        Assert.False(result);
    }

    #endregion

    #region UpdateReservationAsync

    [Fact]
    public async Task UpdateReservationAsync_WhenReservationExists_PersistsChanges()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        context.Restaurants.Add(restaurant);
        var reservation = CreateReservation(restaurant.Id);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var sut = CreateSut(context);

        reservation.Status = ReservationStatus.InProgress;
        await sut.UpdateReservationAsync(reservation);

        var updated = await context.Reservations.FindAsync(reservation.Id);
        Assert.Equal(ReservationStatus.InProgress, updated!.Status);
    }

    [Fact]
    public async Task UpdateReservationAsync_WhenReservationIsNull_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateReservationAsync(null!));
    }

    #endregion

    private static Restaurant CreateRestaurant()
    {
        return new Restaurant
        {
            Id = Guid.NewGuid(),
            City = "Kyiv",
            Address = "Test Address"
        };
    }

    private static Table CreateTable(int tableNumber, Guid restaurantId)
    {
        return new Table
        {
            TableNumber = tableNumber,
            Seats = 4,
            RestaurantId = restaurantId
        };
    }

    private static Reservation CreateReservation(
        Guid restaurantId,
        string? guestId = null,
        string? waiterId = null,
        int? tableNumber = null,
        DateTimeOffset? startTime = null,
        int durationMinutes = 60,
        ReservationStatus status = ReservationStatus.Created)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            GuestId = guestId,
            WaiterId = waiterId,
            TableNumber = tableNumber,
            StartTime = startTime ?? DateTimeOffset.UtcNow.AddHours(2),
            DurationMinutes = durationMinutes,
            NumberOfGuests = 2,
            Status = status
        };
    }

    private static ReservationRepository CreateSut(RestaurantDbContext context)
    {
        return new ReservationRepository(context);
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
