using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Shared.Tables;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class TableMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void TableToTableBrief_MapsTableNumberAndSeats()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var restaurantId = Guid.NewGuid();
        var table = new Table { TableNumber = 5, Seats = 4, RestaurantId = restaurantId };

        var result = mapper.Map<TableBrief>(table);

        Assert.Equal(5, result.TableNumber);
        Assert.Equal(4, result.Seats);
    }

    [Fact]
    public void AddTableRequestToTable_MapsTableNumberAndSeats()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var request = new AddTableRequest { TableNumber = 3, Seats = 2 };

        var result = mapper.Map<Table>(request);

        Assert.Equal(3, result.TableNumber);
        Assert.Equal(2, result.Seats);
    }

    [Fact]
    public void AddTableRequestToTable_DoesNotMapRestaurantId()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var request = new AddTableRequest { TableNumber = 1, Seats = 4 };

        var result = mapper.Map<Table>(request);

        Assert.Equal(Guid.Empty, result.RestaurantId);
    }

    [Fact]
    public void AddTableRequestToTable_DoesNotMapRestaurantNavigation()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var request = new AddTableRequest { TableNumber = 1, Seats = 4 };

        var result = mapper.Map<Table>(request);

        Assert.Null(result.Restaurant);
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(cfg => cfg.AddProfile<TableMappingProfile>());
    }
}
