using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Data;
using RestaurantWithAi.Data.Repositories;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Tests.Data;

[ExcludeFromCodeCoverage]
public class ReservationRepositoryTests
{
    #region GetReservationsByGuestAsync

    [Fact]
    public async Task GetReservationsByGuestAsync_WhenReservationsExist_ReturnsGuestReservations()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var guestUserId = "guest-1";

        context.Reservations.AddRange(
            CreateReservation(restaurant.Id, table.TableNumber, guestUserId, DateTime.UtcNow.AddHours(1), 60),
            CreateReservation(restaurant.Id, table.TableNumber, guestUserId, DateTime.UtcNow.AddHours(5), 60));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetReservationsByGuestAsync(guestUserId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(guestUserId, r.GuestUserId));
    }

    [Fact]
    public async Task GetReservationsByGuestAsync_WhenNoReservationsExist_ReturnsEmpty()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        var result = await sut.GetReservationsByGuestAsync("nonexistent-guest");

        Assert.Empty(result);
    }

    #endregion

    #region GetReservationsByWaiterAsync

    [Fact]
    public async Task GetReservationsByWaiterAsync_WhenReservationsExist_ReturnsWaiterReservations()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var waiter = CreateWaiter("waiter-1", restaurant.Id);
        context.Waiters.Add(waiter);

        var res1 = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", DateTime.UtcNow.AddHours(1), 60);
        res1.AssignedWaiterId = waiter.UserId;
        var res2 = CreateReservation(restaurant.Id, table.TableNumber, "guest-2", DateTime.UtcNow.AddHours(5), 60);
        res2.AssignedWaiterId = waiter.UserId;
        context.Reservations.AddRange(res1, res2);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetReservationsByWaiterAsync(waiter.UserId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(waiter.UserId, r.AssignedWaiterId));
    }

    #endregion

    #region GetReservationByIdAsync

    [Fact]
    public async Task GetReservationByIdAsync_WhenReservationExists_ReturnsReservation()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", DateTime.UtcNow.AddHours(1), 60);
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

    #region CreateReservationAsync

    [Fact]
    public async Task CreateReservationAsync_WhenValid_PersistsReservation()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", DateTime.UtcNow.AddHours(1), 60);

        var sut = CreateSut(context);

        await sut.CreateReservationAsync(reservation);

        var persisted = await context.Reservations.FindAsync(reservation.Id);
        Assert.NotNull(persisted);
        Assert.Equal("guest-1", persisted!.GuestUserId);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenNull_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.CreateReservationAsync(null!));
    }

    [Fact]
    public async Task CreateReservationAsync_WhenTableNotFound_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var (restaurant, _) = await SeedRestaurantAndTable(context, 1);
        var reservation = CreateReservation(restaurant.Id, 99, "guest-1", DateTime.UtcNow.AddHours(1), 60);

        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.CreateReservationAsync(reservation));
    }

    [Fact]
    public async Task CreateReservationAsync_WhenTableIsOccupied_ThrowsReservationConflictException()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var startTime = DateTime.UtcNow.AddHours(2);

        var existing = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", startTime, 60);
        context.Reservations.Add(existing);
        await context.SaveChangesAsync();

        var overlapping = CreateReservation(restaurant.Id, table.TableNumber, "guest-2", startTime.AddMinutes(30), 60);

        var sut = CreateSut(context);

        await Assert.ThrowsAsync<ReservationConflictException>(() => sut.CreateReservationAsync(overlapping));
    }

    [Fact]
    public async Task CreateReservationAsync_WhenStartsWithinBufferPeriod_ThrowsConflict()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var startTime = DateTime.UtcNow.AddHours(2);

        var existing = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", startTime, 60);
        context.Reservations.Add(existing);
        await context.SaveChangesAsync();

        // starts exactly when first ends (no gap) - should conflict
        var overlapping = CreateReservation(restaurant.Id, table.TableNumber, "guest-2", startTime.AddMinutes(60), 60);

        var sut = CreateSut(context);

        await Assert.ThrowsAsync<ReservationConflictException>(() => sut.CreateReservationAsync(overlapping));
    }

    [Fact]
    public async Task CreateReservationAsync_WhenReservationsOnDifferentTables_DoesNotConflict()
    {
        await using var context = CreateContext();
        var (restaurant, table1) = await SeedRestaurantAndTable(context, 1);
        var table2 = new Table { RestaurantId = restaurant.Id, TableNumber = 2, Seats = 4 };
        context.Tables.Add(table2);
        await context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddHours(2);
        var existing = CreateReservation(restaurant.Id, table1.TableNumber, "guest-1", startTime, 60);
        context.Reservations.Add(existing);
        await context.SaveChangesAsync();

        var newReservation = CreateReservation(restaurant.Id, table2.TableNumber, "guest-2", startTime, 60);

        var sut = CreateSut(context);

        await sut.CreateReservationAsync(newReservation);

        Assert.Equal(2, await context.Reservations.CountAsync());
    }

    #endregion

    #region DeleteReservationAsync

    [Fact]
    public async Task DeleteReservationAsync_WhenCreatedStatus_DeletesReservation()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", DateTime.UtcNow.AddHours(1), 60);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await sut.DeleteReservationAsync(reservation.Id);

        Assert.False(await context.Reservations.AnyAsync(r => r.Id == reservation.Id));
    }

    [Fact]
    public async Task DeleteReservationAsync_WhenNotCreatedStatus_ThrowsInvalidOperationException()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", DateTime.UtcNow.AddHours(1), 60);
        reservation.Status = ReservationStatus.InProgress;
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.DeleteReservationAsync(reservation.Id));
    }

    [Fact]
    public async Task DeleteReservationAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.DeleteReservationAsync(Guid.NewGuid()));
    }

    #endregion

    #region UpdateReservationTimeAsync

    [Fact]
    public async Task UpdateReservationTimeAsync_WhenValid_UpdatesTime()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", DateTime.UtcNow.AddHours(1), 60);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var newStartTime = DateTime.UtcNow.AddHours(10);

        var sut = CreateSut(context);

        await sut.UpdateReservationTimeAsync(reservation.Id, newStartTime, 90);

        var updated = await context.Reservations.FindAsync(reservation.Id);
        Assert.Equal(newStartTime, updated!.StartTime);
        Assert.Equal(90, updated.DurationMinutes);
    }

    [Fact]
    public async Task UpdateReservationTimeAsync_WhenOverlap_ThrowsReservationConflictException()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var startTime = DateTime.UtcNow.AddHours(5);

        var reservation1 = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", startTime, 60);
        var reservation2 = CreateReservation(restaurant.Id, table.TableNumber, "guest-2", startTime.AddHours(3), 60);
        context.Reservations.AddRange(reservation1, reservation2);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await Assert.ThrowsAsync<ReservationConflictException>(() =>
            sut.UpdateReservationTimeAsync(reservation1.Id, startTime.AddHours(3), 60));
    }

    #endregion

    #region UpdateReservationTableAsync

    [Fact]
    public async Task UpdateReservationTableAsync_WhenValid_UpdatesTable()
    {
        await using var context = CreateContext();
        var (restaurant, table1) = await SeedRestaurantAndTable(context, 1);
        var table2 = new Table { RestaurantId = restaurant.Id, TableNumber = 2, Seats = 4 };
        context.Tables.Add(table2);
        var reservation = CreateReservation(restaurant.Id, table1.TableNumber, "guest-1", DateTime.UtcNow.AddHours(1), 60);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await sut.UpdateReservationTableAsync(reservation.Id, 2);

        var updated = await context.Reservations.FindAsync(reservation.Id);
        Assert.Equal(2, updated!.TableNumber);
    }

    [Fact]
    public async Task UpdateReservationTableAsync_WhenTableNotFound_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", DateTime.UtcNow.AddHours(1), 60);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateReservationTableAsync(reservation.Id, 99));
    }

    #endregion

    #region UpdateReservationStatusAsync

    [Fact]
    public async Task UpdateReservationStatusAsync_WhenValidTransition_UpdatesStatus()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", DateTime.UtcNow.AddHours(1), 60);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await sut.UpdateReservationStatusAsync(reservation.Id, ReservationStatus.InProgress);

        var updated = await context.Reservations.FindAsync(reservation.Id);
        Assert.Equal(ReservationStatus.InProgress, updated!.Status);
    }

    [Fact]
    public async Task UpdateReservationStatusAsync_WhenInvalidTransition_ThrowsInvalidStatusTransitionException()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", DateTime.UtcNow.AddHours(1), 60);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(() =>
            sut.UpdateReservationStatusAsync(reservation.Id, ReservationStatus.Closed));
    }

    [Theory]
    [InlineData(ReservationStatus.Created, ReservationStatus.InProgress)]
    [InlineData(ReservationStatus.InProgress, ReservationStatus.PendingPayment)]
    [InlineData(ReservationStatus.PendingPayment, ReservationStatus.Closed)]
    public async Task UpdateReservationStatusAsync_ValidTransitions_Succeed(ReservationStatus from, ReservationStatus to)
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", DateTime.UtcNow.AddHours(1), 60);
        reservation.Status = from;
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await sut.UpdateReservationStatusAsync(reservation.Id, to);

        var updated = await context.Reservations.FindAsync(reservation.Id);
        Assert.Equal(to, updated!.Status);
    }

    #endregion

    #region HasOverlapAsync

    [Fact]
    public async Task HasOverlapAsync_WhenNoReservations_ReturnsFalse()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var sut = CreateSut(context);

        var result = await sut.HasOverlapAsync(restaurant.Id, table.TableNumber, DateTime.UtcNow.AddHours(1), 60);

        Assert.False(result);
    }

    [Fact]
    public async Task HasOverlapAsync_WhenExactOverlap_ReturnsTrue()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var startTime = DateTime.UtcNow.AddHours(2);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", startTime, 60);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = await sut.HasOverlapAsync(restaurant.Id, table.TableNumber, startTime, 60);

        Assert.True(result);
    }

    [Fact]
    public async Task HasOverlapAsync_WhenSufficientGap_ReturnsFalse()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var startTime = DateTime.UtcNow.AddHours(2);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", startTime, 60);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        // Start 30 minutes after the previous ends (60 + 15 buffer = 75, so 60 + 30 = 90 min later is safe)
        var result = await sut.HasOverlapAsync(restaurant.Id, table.TableNumber, startTime.AddMinutes(90), 60);

        Assert.False(result);
    }

    [Fact]
    public async Task HasOverlapAsync_WhenClosedReservation_ReturnsFalse()
    {
        await using var context = CreateContext();
        var (restaurant, table) = await SeedRestaurantAndTable(context, 1);
        var startTime = DateTime.UtcNow.AddHours(2);
        var reservation = CreateReservation(restaurant.Id, table.TableNumber, "guest-1", startTime, 60);
        reservation.Status = ReservationStatus.Closed;
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = await sut.HasOverlapAsync(restaurant.Id, table.TableNumber, startTime, 60);

        Assert.False(result);
    }

    #endregion

    private static ReservationRepository CreateSut(RestaurantDbContext context)
    {
        return new ReservationRepository(context);
    }

    private static RestaurantDbContext CreateContext()
    {
        return new TestRestaurantDbContext(Guid.NewGuid().ToString("N"));
    }

    private static async Task<(Restaurant restaurant, Table table)> SeedRestaurantAndTable(RestaurantDbContext context, int tableNumber)
    {
        var restaurant = new Restaurant { Id = Guid.NewGuid(), City = "Kyiv", Address = "Test Address" };
        var table = new Table { RestaurantId = restaurant.Id, TableNumber = tableNumber, Seats = 4 };
        context.Restaurants.Add(restaurant);
        context.Tables.Add(table);
        await context.SaveChangesAsync();
        return (restaurant, table);
    }

    private static Waiter CreateWaiter(string userId, Guid restaurantId)
    {
        return new Waiter { UserId = userId, RestaurantId = restaurantId };
    }

    private static Reservation CreateReservation(Guid restaurantId, int tableNumber, string? guestUserId, DateTime startTime, int durationMinutes)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            TableNumber = tableNumber,
            GuestUserId = guestUserId,
            GuestName = "Test Guest",
            StartTime = startTime,
            DurationMinutes = durationMinutes,
            NumberOfGuests = 2,
            Status = ReservationStatus.Created
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
