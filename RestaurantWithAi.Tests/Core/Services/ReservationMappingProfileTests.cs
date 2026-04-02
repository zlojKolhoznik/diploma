using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using RestaurantWithAi.Core.Mappings;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class ReservationMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ReservationMappingProfile>());
        config.AssertConfigurationIsValid();
    }
}
