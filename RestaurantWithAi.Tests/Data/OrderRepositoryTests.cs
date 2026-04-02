using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Data;
using RestaurantWithAi.Data.Repositories;

namespace RestaurantWithAi.Tests.Data;

[ExcludeFromCodeCoverage]
public class OrderRepositoryTests
{
    [Fact]
    public async Task HasOpenOrderForReservationAsync_WhenOpenOrderExists_ReturnsTrue()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        var reservation = CreateReservation(restaurant.Id);

        context.Restaurants.Add(restaurant);
        context.Reservations.Add(reservation);
        context.Orders.Add(new Order
        {
            RestaurantId = restaurant.Id,
            ReservationId = reservation.Id,
            Status = OrderStatuses.InProgress,
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var sut = new OrderRepository(context);

        var result = await sut.HasOpenOrderForReservationAsync(restaurant.Id, reservation.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task GetOrdersForReservationAsync_WhenOrdersExist_ReturnsOnlyReservationOrders()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant();
        var reservation1 = CreateReservation(restaurant.Id);
        var reservation2 = CreateReservation(restaurant.Id);

        context.Restaurants.Add(restaurant);
        context.Reservations.AddRange(reservation1, reservation2);
        context.Orders.AddRange(
            new Order
            {
                RestaurantId = restaurant.Id,
                ReservationId = reservation1.Id,
                Status = OrderStatuses.Created,
                CreatedAtUtc = DateTime.UtcNow
            },
            new Order
            {
                RestaurantId = restaurant.Id,
                ReservationId = reservation2.Id,
                Status = OrderStatuses.Created,
                CreatedAtUtc = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var sut = new OrderRepository(context);

        var result = (await sut.GetOrdersForReservationAsync(restaurant.Id, reservation1.Id)).ToList();

        Assert.Single(result);
        Assert.Equal(reservation1.Id, result[0].ReservationId);
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

    private static Reservation CreateReservation(Guid restaurantId)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            GuestId = "guest-1",
            StartTime = DateTime.UtcNow,
            ApproximateDurationMinutes = 60,
            NumberOfGuests = 2,
            Status = ReservationStatuses.Created
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

