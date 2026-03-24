using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Moq;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Restaurants;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class RestaurantServiceTests
{
    [Fact]
    public async Task GetRestaurantsAsync_WhenRepositoryReturnsRestaurants_ReturnsMappedRestaurantBriefCollection()
    {
        var repositoryMock = new Mock<IRestaurantRepository>();
        var restaurantOne = CreateRestaurant("Kyiv", "Address 1");
        var restaurantTwo = CreateRestaurant("Lviv", "Address 2");

        repositoryMock
            .Setup(r => r.GetAllRestaurantsAsync(null))
            .ReturnsAsync(new List<Restaurant> { restaurantOne, restaurantTwo });

        var sut = CreateSut(repositoryMock.Object);

        var result = (await sut.GetRestaurantsAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == restaurantOne.Id && r.City == "Kyiv");
        Assert.Contains(result, r => r.Id == restaurantTwo.Id && r.Address == "Address 2");
        repositoryMock.Verify(r => r.GetAllRestaurantsAsync(null), Times.Once);
    }

    [Fact]
    public async Task GetRestaurantsAsync_WhenCityIsProvided_DelegatesFilterToRepository()
    {
        var repositoryMock = new Mock<IRestaurantRepository>();
        var city = "Kyiv";

        repositoryMock
            .Setup(r => r.GetAllRestaurantsAsync(city))
            .ReturnsAsync(new List<Restaurant> { CreateRestaurant(city, "Address 1") });

        var sut = CreateSut(repositoryMock.Object);

        var result = (await sut.GetRestaurantsAsync(city)).ToList();

        Assert.Single(result);
        Assert.Equal(city, result[0].City);
        repositoryMock.Verify(r => r.GetAllRestaurantsAsync(city), Times.Once);
    }

    [Fact]
    public async Task GetRestaurantDetailAsync_WhenRestaurantExists_ReturnsMappedRestaurantDetail()
    {
        var repositoryMock = new Mock<IRestaurantRepository>();
        var restaurant = CreateRestaurant("Kyiv", "Address 1");
        restaurant.AvailableDishes.Add(new Dish
        {
            Id = Guid.NewGuid(),
            Name = "Pizza",
            Description = "Cheese pizza",
            Price = 10m,
            ImageUrl = "https://example.com/pizza.jpg"
        });

        repositoryMock
            .Setup(r => r.GetRestaurantByIdAsync(restaurant.Id))
            .ReturnsAsync(restaurant);

        var sut = CreateSut(repositoryMock.Object);

        var result = await sut.GetRestaurantDetailAsync(restaurant.Id);

        Assert.Equal(restaurant.Id, result.Id);
        Assert.Equal("Kyiv", result.City);
        Assert.Single(result.AvailableDishes);
        repositoryMock.Verify(r => r.GetRestaurantByIdAsync(restaurant.Id), Times.Once);
    }

    [Fact]
    public async Task CreateRestaurantAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<IRestaurantRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.CreateRestaurantAsync(null!));
        repositoryMock.Verify(r => r.AddRestaurantAsync(It.IsAny<Restaurant>()), Times.Never);
    }

    [Fact]
    public async Task CreateRestaurantAsync_WhenRequestIsValid_MapsAndDelegatesToRepository()
    {
        var repositoryMock = new Mock<IRestaurantRepository>();
        Restaurant? capturedRestaurant = null;

        repositoryMock
            .Setup(r => r.AddRestaurantAsync(It.IsAny<Restaurant>()))
            .Callback<Restaurant>(restaurant => capturedRestaurant = restaurant)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.CreateRestaurantAsync(new CreateRestaurantRequest
        {
            City = "Kyiv",
            Address = "Address 1"
        });

        Assert.NotNull(capturedRestaurant);
        Assert.Equal("Kyiv", capturedRestaurant!.City);
        Assert.Equal("Address 1", capturedRestaurant.Address);
        Assert.Empty(capturedRestaurant.AvailableDishes);
        repositoryMock.Verify(r => r.AddRestaurantAsync(It.IsAny<Restaurant>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRestaurantAsync_WhenRequestIsValid_UsesProvidedIdAndDelegatesToRepository()
    {
        var repositoryMock = new Mock<IRestaurantRepository>();
        Restaurant? capturedRestaurant = null;
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.UpdateRestaurantAsync(It.IsAny<Restaurant>()))
            .Callback<Restaurant>(restaurant => capturedRestaurant = restaurant)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.UpdateRestaurantAsync(restaurantId, new CreateRestaurantRequest
        {
            City = "Dnipro",
            Address = "Address 9"
        });

        Assert.NotNull(capturedRestaurant);
        Assert.Equal(restaurantId, capturedRestaurant!.Id);
        Assert.Equal("Dnipro", capturedRestaurant.City);
        Assert.Equal("Address 9", capturedRestaurant.Address);
        Assert.Empty(capturedRestaurant.AvailableDishes);
        repositoryMock.Verify(r => r.UpdateRestaurantAsync(It.IsAny<Restaurant>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRestaurantAsync_WhenCalled_DelegatesToRepository()
    {
        var repositoryMock = new Mock<IRestaurantRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.DeleteRestaurantAsync(restaurantId))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.DeleteRestaurantAsync(restaurantId);

        repositoryMock.Verify(r => r.DeleteRestaurantAsync(restaurantId), Times.Once);
    }

    private static RestaurantService CreateSut(IRestaurantRepository repository)
    {
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DishMappingProfile>();
            cfg.AddProfile<RestaurantMappingProfile>();
        });
        var mapper = mapperConfiguration.CreateMapper();
        return new RestaurantService(repository, mapper);
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
}

