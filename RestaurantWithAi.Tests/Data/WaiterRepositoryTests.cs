using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Data;
using RestaurantWithAi.Data.Repositories;

namespace RestaurantWithAi.Tests.Data;

[ExcludeFromCodeCoverage]
public class WaiterRepositoryTests
{
    [Fact]
    public async Task GetAllWaitersAsync_WhenWaitersExist_ReturnsAllWaiters()
    {
        await using var context = CreateContext();
        context.Waiters.AddRange(CreateWaiter("user1"), CreateWaiter("user2"));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetAllWaitersAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, w => w.UserId == "user1");
        Assert.Contains(result, w => w.UserId == "user2");
    }

    [Fact]
    public async Task GetAllWaitersAsync_WhenNoWaitersExist_ReturnsEmptyCollection()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        var result = await sut.GetAllWaitersAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllWaitersAsync_WhenCalled_DoesNotTrackEntities()
    {
        await using var context = CreateContext();
        context.Waiters.AddRange(CreateWaiter("user1"), CreateWaiter("user2"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var sut = CreateSut(context);

        var result = (await sut.GetAllWaitersAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetWaiterByUserIdAsync_WhenWaiterExists_ReturnsWaiter()
    {
        await using var context = CreateContext();
        var waiter = CreateWaiter("user1");
        context.Waiters.Add(waiter);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = await sut.GetWaiterByUserIdAsync("user1");

        Assert.Equal("user1", result.UserId);
    }

    [Fact]
    public async Task GetWaiterByUserIdAsync_WhenCalled_DoesNotTrackEntity()
    {
        await using var context = CreateContext();
        var waiter = CreateWaiter("user1");
        context.Waiters.Add(waiter);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var sut = CreateSut(context);

        _ = await sut.GetWaiterByUserIdAsync("user1");

        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetWaiterByUserIdAsync_WhenWaiterDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetWaiterByUserIdAsync("unknown"));
    }

    [Fact]
    public async Task AddWaiterAsync_WhenWaiterIsValid_PersistsWaiter()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);
        var waiter = CreateWaiter("user1");

        await sut.AddWaiterAsync(waiter);

        var persisted = await context.Waiters.SingleAsync(w => w.UserId == "user1");
        Assert.Equal("user1", persisted.UserId);
    }

    [Fact]
    public async Task AddWaiterAsync_WhenWaiterIsNull_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddWaiterAsync(null!));
    }

    [Fact]
    public async Task UpdateWaiterRestaurantAsync_WhenWaiterExists_UpdatesRestaurantId()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Street 1");
        var waiter = CreateWaiter("user1");
        context.Restaurants.Add(restaurant);
        context.Waiters.Add(waiter);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await sut.UpdateWaiterRestaurantAsync("user1", restaurant.Id);

        var persisted = await context.Waiters.SingleAsync(w => w.UserId == "user1");
        Assert.Equal(restaurant.Id, persisted.RestaurantId);
    }

    [Fact]
    public async Task UpdateWaiterRestaurantAsync_WhenSettingRestaurantIdToNull_ClearsRestaurantId()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Street 1");
        var waiter = CreateWaiter("user1", restaurant.Id);
        context.Restaurants.Add(restaurant);
        context.Waiters.Add(waiter);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await sut.UpdateWaiterRestaurantAsync("user1", null);

        var persisted = await context.Waiters.SingleAsync(w => w.UserId == "user1");
        Assert.Null(persisted.RestaurantId);
    }

    [Fact]
    public async Task UpdateWaiterRestaurantAsync_WhenWaiterDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateWaiterRestaurantAsync("unknown", Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteWaiterAsync_WhenWaiterExists_RemovesWaiter()
    {
        await using var context = CreateContext();
        var waiter = CreateWaiter("user1");
        context.Waiters.Add(waiter);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await sut.DeleteWaiterAsync("user1");

        var exists = await context.Waiters.AnyAsync(w => w.UserId == "user1");
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteWaiterAsync_WhenWaiterDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.DeleteWaiterAsync("unknown"));
    }

    private static WaiterRepository CreateSut(RestaurantDbContext context)
    {
        return new WaiterRepository(context);
    }

    private static RestaurantDbContext CreateContext()
    {
        return new TestRestaurantDbContext(Guid.NewGuid().ToString("N"));
    }

    private static Waiter CreateWaiter(string userId, Guid? restaurantId = null)
    {
        return new Waiter { UserId = userId, RestaurantId = restaurantId };
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
