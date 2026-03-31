using System.Diagnostics.CodeAnalysis;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using AutoMapper;
using Microsoft.Extensions.Options;
using Moq;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Options;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class WaiterServiceTests
{
    private static readonly AwsCognitoOptions ValidOptions = new()
    {
        Region = "eu-central-1",
        UserPoolId = "pool-id",
        ClientId = "client-id",
        Authority = "authority",
        ClientSecret = "client-secret"
    };

    #region GetAllWaitersAsync

    [Fact]
    public async Task GetAllWaitersAsync_WhenNoUsers_ReturnsEmptyCollection()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        cognitoMock
            .Setup(c => c.ListUsersInGroupAsync(It.IsAny<ListUsersInGroupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListUsersInGroupResponse { Users = [] });

        var sut = CreateService(cognitoMock.Object);

        // Act
        var result = await sut.GetAllWaitersAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllWaitersAsync_WhenUsersExist_ReturnsMappedWaiters()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        var users = new List<UserType>
        {
            CreateUserType("user1", "waiter1@example.com", "John", "Doe", "rest-1"),
            CreateUserType("user2", "waiter2@example.com", "Jane", "Smith", null)
        };

        cognitoMock
            .Setup(c => c.ListUsersInGroupAsync(It.IsAny<ListUsersInGroupRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListUsersInGroupResponse { Users = users });

        var sut = CreateService(cognitoMock.Object);

        // Act
        var result = (await sut.GetAllWaitersAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        var first = result[0];
        Assert.Equal("user1", first.UserId);
        Assert.Equal("waiter1@example.com", first.Email);
        Assert.Equal("John", first.FirstName);
        Assert.Equal("Doe", first.LastName);
        Assert.Equal("rest-1", first.RestaurantId);

        var second = result[1];
        Assert.Equal("user2", second.UserId);
        Assert.Null(second.RestaurantId);
    }

    [Fact]
    public async Task GetAllWaitersAsync_SendsCorrectRequestToCognito()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        ListUsersInGroupRequest? capturedRequest = null;

        cognitoMock
            .Setup(c => c.ListUsersInGroupAsync(It.IsAny<ListUsersInGroupRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ListUsersInGroupRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new ListUsersInGroupResponse { Users = [] });

        var sut = CreateService(cognitoMock.Object);

        // Act
        await sut.GetAllWaitersAsync();

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(ValidOptions.UserPoolId, capturedRequest!.UserPoolId);
        Assert.Equal("Waiters", capturedRequest.GroupName);
    }

    #endregion

    #region AssignWaiterRoleAsync

    [Fact]
    public async Task AssignWaiterRoleAsync_WhenUserExists_CallsAdminAddUserToGroup()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        AdminAddUserToGroupRequest? capturedRequest = null;

        cognitoMock
            .Setup(c => c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminGetUserResponse());

        cognitoMock
            .Setup(c => c.AdminAddUserToGroupAsync(It.IsAny<AdminAddUserToGroupRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AdminAddUserToGroupRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new AdminAddUserToGroupResponse());

        var sut = CreateService(cognitoMock.Object);

        // Act
        await sut.AssignWaiterRoleAsync("user1");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(ValidOptions.UserPoolId, capturedRequest!.UserPoolId);
        Assert.Equal("user1", capturedRequest.Username);
        Assert.Equal("Waiters", capturedRequest.GroupName);
    }

    [Fact]
    public async Task AssignWaiterRoleAsync_WhenUserNotFound_ThrowsUserNotFoundExceptionAndDoesNotAddToGroup()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();

        cognitoMock
            .Setup(c => c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Amazon.CognitoIdentityProvider.Model.UserNotFoundException("User not found"));

        var sut = CreateService(cognitoMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<RestaurantWithAi.Shared.Exceptions.UserNotFoundException>(() => sut.AssignWaiterRoleAsync("unknown"));
        cognitoMock.Verify(c => c.AdminAddUserToGroupAsync(It.IsAny<AdminAddUserToGroupRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region AssignWaiterToRestaurantAsync

    [Fact]
    public async Task AssignWaiterToRestaurantAsync_WhenUserExists_SetsRestaurantIdAttribute()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        AdminUpdateUserAttributesRequest? capturedRequest = null;

        cognitoMock
            .Setup(c => c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminGetUserResponse());

        cognitoMock
            .Setup(c => c.AdminUpdateUserAttributesAsync(It.IsAny<AdminUpdateUserAttributesRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AdminUpdateUserAttributesRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new AdminUpdateUserAttributesResponse());

        var sut = CreateService(cognitoMock.Object);

        // Act
        await sut.AssignWaiterToRestaurantAsync("rest-42", "user1");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(ValidOptions.UserPoolId, capturedRequest!.UserPoolId);
        Assert.Equal("user1", capturedRequest.Username);
        Assert.Single(capturedRequest.UserAttributes);
        Assert.Equal("custom:restaurantId", capturedRequest.UserAttributes[0].Name);
        Assert.Equal("rest-42", capturedRequest.UserAttributes[0].Value);
    }

    [Fact]
    public async Task AssignWaiterToRestaurantAsync_WhenUserNotFound_ThrowsUserNotFoundExceptionAndDoesNotUpdate()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();

        cognitoMock
            .Setup(c => c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Amazon.CognitoIdentityProvider.Model.UserNotFoundException("User not found"));

        var sut = CreateService(cognitoMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<RestaurantWithAi.Shared.Exceptions.UserNotFoundException>(() => sut.AssignWaiterToRestaurantAsync("rest-1", "unknown"));
        cognitoMock.Verify(c => c.AdminUpdateUserAttributesAsync(It.IsAny<AdminUpdateUserAttributesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    private static WaiterService CreateService(IAmazonCognitoIdentityProvider cognito)
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<WaiterMappingProfile>()).CreateMapper();
        return new WaiterService(cognito, Options.Create(ValidOptions), mapper);
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
