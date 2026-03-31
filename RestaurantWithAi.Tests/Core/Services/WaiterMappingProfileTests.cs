using System.Diagnostics.CodeAnalysis;
using Amazon.CognitoIdentityProvider.Model;
using AutoMapper;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class WaiterMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void UserTypeToWaiterResponse_MapsUsernameToUserId()
    {
        var mapper = CreateMapper();
        var userType = CreateUserType("user-123", "test@example.com", "Alice", "Doe", "rest-1");

        var result = mapper.Map<WaiterResponse>(userType);

        Assert.Equal("user-123", result.UserId);
    }

    [Fact]
    public void UserTypeToWaiterResponse_MapsAllAttributes()
    {
        var mapper = CreateMapper();
        var userType = CreateUserType("user-456", "alice@example.com", "Alice", "Wonderland", "rest-99");

        var result = mapper.Map<WaiterResponse>(userType);

        Assert.Equal("alice@example.com", result.Email);
        Assert.Equal("Alice", result.FirstName);
        Assert.Equal("Wonderland", result.LastName);
        Assert.Equal("rest-99", result.RestaurantId);
    }

    [Fact]
    public void UserTypeToWaiterResponse_WhenOptionalAttributesAbsent_MapsToNull()
    {
        var mapper = CreateMapper();
        var userType = new UserType
        {
            Username = "user-789",
            Attributes = [new() { Name = "email", Value = "user@example.com" }]
        };

        var result = mapper.Map<WaiterResponse>(userType);

        Assert.Null(result.FirstName);
        Assert.Null(result.LastName);
        Assert.Null(result.RestaurantId);
    }

    [Fact]
    public void UserTypeToWaiterResponse_WhenEmailAttributeAbsent_MapsToEmptyString()
    {
        var mapper = CreateMapper();
        var userType = new UserType
        {
            Username = "user-000",
            Attributes = []
        };

        var result = mapper.Map<WaiterResponse>(userType);

        Assert.Equal(string.Empty, result.Email);
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(cfg => cfg.AddProfile<WaiterMappingProfile>());
    }

    private static IMapper CreateMapper()
    {
        return CreateConfiguration().CreateMapper();
    }

    private static UserType CreateUserType(string username, string email, string? firstName, string? lastName, string? restaurantId)
    {
        var attributes = new List<AttributeType>
        {
            new() { Name = "email", Value = email }
        };

        if (firstName is not null)
            attributes.Add(new() { Name = "given_name", Value = firstName });
        if (lastName is not null)
            attributes.Add(new() { Name = "family_name", Value = lastName });
        if (restaurantId is not null)
            attributes.Add(new() { Name = "custom:restaurantId", Value = restaurantId });

        return new UserType { Username = username, Attributes = attributes };
    }
}
