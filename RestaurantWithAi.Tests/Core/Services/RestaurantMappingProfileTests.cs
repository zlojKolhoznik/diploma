using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Shared.Restaurants;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class RestaurantMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void CreateRestaurantRequestToRestaurant_DoesNotMapAvailableDishes()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var request = new CreateRestaurantRequest
        {
            City = "Kyiv",
            Address = "Address 1"
        };

        var result = mapper.Map<Restaurant>(request);

        Assert.Equal("Kyiv", result.City);
        Assert.Equal("Address 1", result.Address);
        Assert.Empty(result.AvailableDishes);
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DishMappingProfile>();
            cfg.AddProfile<RestaurantMappingProfile>();
        });
    }
}

