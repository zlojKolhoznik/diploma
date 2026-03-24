using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Moq;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Dishes;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class DishServiceTests
{
    [Fact]
    public async Task GetDishesAsync_WhenRepositoryReturnsDishes_ReturnsMappedDishBriefCollection()
    {
        // Arrange
        var repositoryMock = new Mock<IDishRepository>();
        var dishOne = CreateDish("Pizza", 12.50m);
        var dishTwo = CreateDish("Pasta", 10.00m);

        repositoryMock
            .Setup(r => r.GetAllDishesAsync())
            .ReturnsAsync(new List<Dish> { dishOne, dishTwo });

        var sut = CreateSut(repositoryMock.Object);

        // Act
        var result = (await sut.GetDishesAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.Id == dishOne.Id && d.Name == "Pizza" && d.Price == 12.50m);
        Assert.Contains(result, d => d.Id == dishTwo.Id && d.Name == "Pasta" && d.ImageUrl == dishTwo.ImageUrl);
        repositoryMock.Verify(r => r.GetAllDishesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetDishDetailAsync_WhenRepositoryReturnsDish_ReturnsMappedDishDetail()
    {
        // Arrange
        var repositoryMock = new Mock<IDishRepository>();
        var dish = CreateDish("Burger", 9.99m);

        repositoryMock
            .Setup(r => r.GetDishByIdAsync(dish.Id))
            .ReturnsAsync(dish);

        var sut = CreateSut(repositoryMock.Object);

        // Act
        var result = await sut.GetDishDetailAsync(dish.Id);

        // Assert
        Assert.Equal(dish.Id, result.Id);
        Assert.Equal("Burger", result.Name);
        Assert.Equal(dish.Description, result.Description);
        Assert.Equal(9.99m, result.Price);
        Assert.Equal(dish.ImageUrl, result.ImageUrl);
        repositoryMock.Verify(r => r.GetDishByIdAsync(dish.Id), Times.Once);
    }

    [Fact]
    public async Task GetDishDetailAsync_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        var repositoryMock = new Mock<IDishRepository>();
        var dishId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetDishByIdAsync(dishId))
            .ThrowsAsync(new KeyNotFoundException("not found"));

        var sut = CreateSut(repositoryMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetDishDetailAsync(dishId));
    }

    [Fact]
    public async Task CreateDishAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var repositoryMock = new Mock<IDishRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.CreateDishAsync(null!));
        repositoryMock.Verify(r => r.AddDishAsync(It.IsAny<Dish>()), Times.Never);
    }

    [Fact]
    public async Task CreateDishAsync_WhenRequestIsValid_MapsAndDelegatesToRepository()
    {
        // Arrange
        var repositoryMock = new Mock<IDishRepository>();
        Dish? capturedDish = null;

        repositoryMock
            .Setup(r => r.AddDishAsync(It.IsAny<Dish>()))
            .Callback<Dish>(dish => capturedDish = dish)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);
        var request = CreateRequest("Sushi", "Fresh sushi set", 21.5m, "https://example.com/sushi.jpg");

        // Act
        await sut.CreateDishAsync(request);

        // Assert
        Assert.NotNull(capturedDish);
        Assert.Equal("Sushi", capturedDish!.Name);
        Assert.Equal("Fresh sushi set", capturedDish.Description);
        Assert.Equal(21.5m, capturedDish.Price);
        Assert.Equal("https://example.com/sushi.jpg", capturedDish.ImageUrl);
        repositoryMock.Verify(r => r.AddDishAsync(It.IsAny<Dish>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDishAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var repositoryMock = new Mock<IDishRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateDishAsync(Guid.NewGuid(), null!));
        repositoryMock.Verify(r => r.UpdateDishAsync(It.IsAny<Dish>()), Times.Never);
    }

    [Fact]
    public async Task UpdateDishAsync_WhenRequestIsValid_UsesProvidedIdAndDelegatesToRepository()
    {
        // Arrange
        var repositoryMock = new Mock<IDishRepository>();
        Dish? capturedDish = null;
        var dishId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.UpdateDishAsync(It.IsAny<Dish>()))
            .Callback<Dish>(dish => capturedDish = dish)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);
        var request = CreateRequest("Soup", "Warm tomato soup", 8m, "https://example.com/soup.jpg");

        // Act
        await sut.UpdateDishAsync(dishId, request);

        // Assert
        Assert.NotNull(capturedDish);
        Assert.Equal(dishId, capturedDish!.Id);
        Assert.Equal("Soup", capturedDish.Name);
        Assert.Equal("Warm tomato soup", capturedDish.Description);
        Assert.Equal(8m, capturedDish.Price);
        Assert.Equal("https://example.com/soup.jpg", capturedDish.ImageUrl);
        repositoryMock.Verify(r => r.UpdateDishAsync(It.IsAny<Dish>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDishAsync_WhenCalled_DelegatesToRepository()
    {
        // Arrange
        var repositoryMock = new Mock<IDishRepository>();
        var dishId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.DeleteDishAsync(dishId))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        // Act
        await sut.DeleteDishAsync(dishId);

        // Assert
        repositoryMock.Verify(r => r.DeleteDishAsync(dishId), Times.Once);
    }

    private static Dish CreateDish(string name, decimal price)
    {
        return new Dish
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Description for {name}",
            Price = price,
            ImageUrl = "https://example.com/dish.jpg"
        };
    }

    private static CreateDishRequest CreateRequest(string name, string description, decimal price, string imageUrl)
    {
        return new CreateDishRequest
        {
            Name = name,
            Description = description,
            Price = price,
            ImageUrl = imageUrl
        };
    }

    private static DishService CreateSut(IDishRepository repository)
    {
        var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<DishMappingProfile>());
        var mapper = mapperConfiguration.CreateMapper();
        return new DishService(repository, mapper);
    }
}

