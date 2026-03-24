using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Data;
using RestaurantWithAi.Data.Repositories;

namespace RestaurantWithAi.Tests.Data;

[ExcludeFromCodeCoverage]
public class RestaurantRepositoryTests
{
    [Fact]
    public async Task GetAllRestaurantsAsync_WhenRestaurantsExist_ReturnsAllRestaurants()
    {
        await using var context = CreateContext();
        context.Restaurants.AddRange(CreateRestaurant("Kyiv", "Address 1"), CreateRestaurant("Lviv", "Address 2"));
        await context.SaveChangesAsync();

        var sut = new RestaurantRepository(context);

        var result = (await sut.GetAllRestaurantsAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllRestaurantsAsync_WhenCityFilterIsProvided_ReturnsOnlyMatchingRestaurants()
    {
        await using var context = CreateContext();
        context.Restaurants.AddRange(
            CreateRestaurant("Kyiv", "Address 1"),
            CreateRestaurant("Lviv", "Address 2"),
            CreateRestaurant("Kyiv", "Address 3"));
        await context.SaveChangesAsync();

        var sut = new RestaurantRepository(context);

        var result = (await sut.GetAllRestaurantsAsync("Kyiv")).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, restaurant => Assert.Equal("Kyiv", restaurant.City));
    }

    [Fact]
    public async Task GetAllRestaurantsAsync_WhenCityFilterHasDifferentCasing_ReturnsOnlyMatchingRestaurants()
    {
        await using var context = CreateContext();
        context.Restaurants.AddRange(
            CreateRestaurant("Kyiv", "Address 1"),
            CreateRestaurant("Lviv", "Address 2"),
            CreateRestaurant("KYIV", "Address 3"));
        await context.SaveChangesAsync();

        var sut = new RestaurantRepository(context);

        var result = (await sut.GetAllRestaurantsAsync("kYiV")).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, restaurant => Assert.Equal("KYIV", restaurant.City.ToUpper()));
    }

    [Fact]
    public async Task GetAllRestaurantsAsync_WhenCityFilterIsEmpty_ReturnsAllRestaurants()
    {
        await using var context = CreateContext();
        context.Restaurants.AddRange(CreateRestaurant("Kyiv", "Address 1"), CreateRestaurant("Lviv", "Address 2"));
        await context.SaveChangesAsync();

        var sut = new RestaurantRepository(context);

        var result = (await sut.GetAllRestaurantsAsync(string.Empty)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllRestaurantsAsync_WhenCityFilterIsWhitespace_ReturnsAllRestaurants()
    {
        await using var context = CreateContext();
        context.Restaurants.AddRange(CreateRestaurant("Kyiv", "Address 1"), CreateRestaurant("Lviv", "Address 2"));
        await context.SaveChangesAsync();

        var sut = new RestaurantRepository(context);

        var result = (await sut.GetAllRestaurantsAsync("   ")).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetRestaurantByIdAsync_WhenRestaurantExists_ReturnsRestaurant()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        var sut = new RestaurantRepository(context);

        var result = await sut.GetRestaurantByIdAsync(restaurant.Id);

        Assert.Equal(restaurant.Id, result.Id);
        Assert.Equal("Kyiv", result.City);
    }

    [Fact]
    public async Task GetRestaurantByIdAsync_WhenRestaurantDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = new RestaurantRepository(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetRestaurantByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task AddRestaurantAsync_WhenRestaurantIsValid_PersistsRestaurant()
    {
        await using var context = CreateContext();
        var sut = new RestaurantRepository(context);
        var restaurant = CreateRestaurant("Kyiv", "Address 1");

        await sut.AddRestaurantAsync(restaurant);

        var persistedRestaurant = await context.Restaurants.SingleAsync(r => r.Id == restaurant.Id);
        Assert.Equal("Kyiv", persistedRestaurant.City);
    }

    [Fact]
    public async Task UpdateRestaurantAsync_WhenRestaurantExists_UpdatesRestaurant()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        var sut = new RestaurantRepository(context);

        await sut.UpdateRestaurantAsync(new Restaurant
        {
            Id = restaurant.Id,
            City = "Odesa",
            Address = "Address 99"
        });

        var persistedRestaurant = await context.Restaurants.SingleAsync(r => r.Id == restaurant.Id);
        Assert.Equal("Odesa", persistedRestaurant.City);
        Assert.Equal("Address 99", persistedRestaurant.Address);
    }

    [Fact]
    public async Task DeleteRestaurantAsync_WhenRestaurantExists_RemovesRestaurant()
    {
        await using var context = CreateContext();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        var sut = new RestaurantRepository(context);

        await sut.DeleteRestaurantAsync(restaurant.Id);

        var exists = await context.Restaurants.AnyAsync(r => r.Id == restaurant.Id);
        Assert.False(exists);
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

