using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Shared.Dishes;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class DishMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void DishToDishDetail_MapsExpectedFields()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var dish = new Dish
        {
            Id = Guid.NewGuid(),
            Name = "Ramen",
            Description = "Miso ramen",
            Price = 15.5m,
            ImageUrl = "https://example.com/ramen.jpg"
        };

        var result = mapper.Map<DishDetail>(dish);

        Assert.Equal(dish.Id, result.Id);
        Assert.Equal(dish.Name, result.Name);
        Assert.Equal(dish.Description, result.Description);
        Assert.Equal(dish.Price, result.Price);
        Assert.Equal(dish.ImageUrl, result.ImageUrl);
    }

    [Fact]
    public void CreateDishRequestToDish_MapsExpectedFields()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var request = new CreateDishRequest
        {
            Name = "Tiramisu",
            Description = "Italian dessert",
            Price = 7.25m,
            ImageUrl = "https://example.com/tiramisu.jpg"
        };

        var result = mapper.Map<Dish>(request);

        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.Price, result.Price);
        Assert.Equal(request.ImageUrl, result.ImageUrl);
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(cfg => cfg.AddProfile<DishMappingProfile>());
    }
}

