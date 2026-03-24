using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Data;
using RestaurantWithAi.Data.Repositories;

namespace RestaurantWithAi.Tests.Data;

[ExcludeFromCodeCoverage]
public class DishRepositoryTests
{
    [Fact]
    public async Task GetAllDishesAsync_WhenDishesExist_ReturnsAllDishes()
    {
        await using var context = CreateContext();
        context.Dishes.AddRange(CreateDish("Pizza"), CreateDish("Pasta"));
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = (await sut.GetAllDishesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.Name == "Pizza");
        Assert.Contains(result, d => d.Name == "Pasta");
    }

    [Fact]
    public async Task GetAllDishesAsync_WhenNoDishesExist_ReturnsEmptyCollection()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        var result = await sut.GetAllDishesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDishByIdAsync_WhenDishExists_ReturnsDish()
    {
        await using var context = CreateContext();
        var dish = CreateDish("Burger");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var result = await sut.GetDishByIdAsync(dish.Id);

        Assert.Equal(dish.Id, result.Id);
        Assert.Equal("Burger", result.Name);
    }

    [Fact]
    public async Task GetDishByIdAsync_WhenDishDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetDishByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task AddDishAsync_WhenDishIsValid_PersistsDish()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);
        var dish = CreateDish("Taco");

        await sut.AddDishAsync(dish);

        var persistedDish = await context.Dishes.SingleAsync(d => d.Id == dish.Id);
        Assert.Equal("Taco", persistedDish.Name);
    }

    [Fact]
    public async Task AddDishAsync_WhenDishIsNull_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddDishAsync(null!));
    }

    [Fact]
    public async Task UpdateDishAsync_WhenDishExists_UpdatesPersistedDish()
    {
        await using var context = CreateContext();
        var originalDish = CreateDish("Salad");
        context.Dishes.Add(originalDish);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);
        var updatedDish = new Dish
        {
            Id = originalDish.Id,
            Name = "Greek Salad",
            Description = "Fresh salad with feta cheese",
            Price = 12.99m,
            ImageUrl = "https://example.com/greek-salad.jpg"
        };

        await sut.UpdateDishAsync(updatedDish);

        var persistedDish = await context.Dishes.SingleAsync(d => d.Id == originalDish.Id);
        Assert.Equal("Greek Salad", persistedDish.Name);
        Assert.Equal(12.99m, persistedDish.Price);
    }

    [Fact]
    public async Task UpdateDishAsync_WhenDishIsNull_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateDishAsync(null!));
    }

    [Fact]
    public async Task UpdateDishAsync_WhenDishDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);
        var dish = CreateDish("Soup");

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateDishAsync(dish));
    }

    [Fact]
    public async Task DeleteDishAsync_WhenDishExists_RemovesDish()
    {
        await using var context = CreateContext();
        var dish = CreateDish("Risotto");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        await sut.DeleteDishAsync(dish.Id);

        var exists = await context.Dishes.AnyAsync(d => d.Id == dish.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteDishAsync_WhenDishDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.DeleteDishAsync(Guid.NewGuid()));
    }

    private static DishRepository CreateSut(RestaurantDbContext context)
    {
        var loggerMock = new Mock<ILogger<DishRepository>>();
        return new DishRepository(context, loggerMock.Object);
    }

    private static RestaurantDbContext CreateContext()
    {
        return new TestRestaurantDbContext(Guid.NewGuid().ToString("N"));
    }

    private static Dish CreateDish(string name)
    {
        return new Dish
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Description for {name}",
            Price = 10m,
            ImageUrl = "https://example.com/dish.jpg"
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