using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantWithAi.Api.Controllers;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Tests.Controllers;

public class WaitersControllerTests
{
    private readonly Mock<IWaiterService> _waiterServiceMock;
    private readonly Mock<ILogger<WaitersController>> _loggerMock;
    private readonly WaitersController _controller;

    public WaitersControllerTests()
    {
        _waiterServiceMock = new Mock<IWaiterService>();
        _loggerMock = new Mock<ILogger<WaitersController>>();
        _controller = new WaitersController(_waiterServiceMock.Object, _loggerMock.Object);
    }

    #region GetAllWaiters

    [Fact]
    public async Task GetAllWaiters_ReturnsOkWithWaiters_WhenServiceSucceeds()
    {
        // Arrange
        var waiters = new List<WaiterResponse>
        {
            new() { UserId = "user1", Email = "waiter1@example.com", FirstName = "John", LastName = "Doe" },
            new() { UserId = "user2", Email = "waiter2@example.com", FirstName = "Jane", LastName = "Smith" }
        };
        _waiterServiceMock.Setup(s => s.GetAllWaitersAsync()).ReturnsAsync(waiters);

        // Act
        var result = await _controller.GetAllWaiters();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedWaiters = Assert.IsAssignableFrom<IEnumerable<WaiterResponse>>(okResult.Value);
        Assert.Equal(2, returnedWaiters.Count());
    }

    [Fact]
    public async Task GetAllWaiters_Returns500_WhenServiceThrows()
    {
        // Arrange
        _waiterServiceMock.Setup(s => s.GetAllWaitersAsync()).ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetAllWaiters();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    #endregion

    #region AssignWaiterRole

    [Fact]
    public async Task AssignWaiterRole_ReturnsNoContent_WhenServiceSucceeds()
    {
        // Arrange
        var userId = "user123";
        var request = new AssignWaiterRoleRequest();
        _waiterServiceMock.Setup(s => s.AssignWaiterRoleAsync(userId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AssignWaiterRole(userId, request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AssignWaiterRole_AlsoAssignsRestaurant_WhenRestaurantIdProvided()
    {
        // Arrange
        var userId = "user123";
        var restaurantId = "restaurant1";
        var request = new AssignWaiterRoleRequest { RestaurantId = restaurantId };
        _waiterServiceMock.Setup(s => s.AssignWaiterRoleAsync(userId)).Returns(Task.CompletedTask);
        _waiterServiceMock.Setup(s => s.AssignWaiterToRestaurantAsync(restaurantId, userId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AssignWaiterRole(userId, request);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _waiterServiceMock.Verify(s => s.AssignWaiterToRestaurantAsync(restaurantId, userId), Times.Once);
    }

    [Fact]
    public async Task AssignWaiterRole_DoesNotAssignRestaurant_WhenRestaurantIdNotProvided()
    {
        // Arrange
        var userId = "user123";
        var request = new AssignWaiterRoleRequest();
        _waiterServiceMock.Setup(s => s.AssignWaiterRoleAsync(userId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AssignWaiterRole(userId, request);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _waiterServiceMock.Verify(s => s.AssignWaiterToRestaurantAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AssignWaiterRole_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var userId = "nonexistent";
        var request = new AssignWaiterRoleRequest();
        _waiterServiceMock.Setup(s => s.AssignWaiterRoleAsync(userId))
            .ThrowsAsync(new UserNotFoundException($"User with id '{userId}' was not found."));

        // Act
        var result = await _controller.AssignWaiterRole(userId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task AssignWaiterRole_Returns500_WhenServiceThrows()
    {
        // Arrange
        var userId = "user123";
        var request = new AssignWaiterRoleRequest();
        _waiterServiceMock.Setup(s => s.AssignWaiterRoleAsync(userId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.AssignWaiterRole(userId, request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    #endregion

    #region AssignWaiterToRestaurant

    [Fact]
    public async Task AssignWaiterToRestaurant_ReturnsNoContent_WhenServiceSucceeds()
    {
        // Arrange
        var request = new AssignWaiterToRestaurantRequest { UserId = "user123", RestaurantId = "restaurant1" };
        _waiterServiceMock.Setup(s => s.AssignWaiterToRestaurantAsync(request.RestaurantId, request.UserId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AssignWaiterToRestaurant(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AssignWaiterToRestaurant_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var request = new AssignWaiterToRestaurantRequest { UserId = "nonexistent", RestaurantId = "restaurant1" };
        _waiterServiceMock.Setup(s => s.AssignWaiterToRestaurantAsync(request.RestaurantId, request.UserId))
            .ThrowsAsync(new UserNotFoundException($"User with id '{request.UserId}' was not found."));

        // Act
        var result = await _controller.AssignWaiterToRestaurant(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task AssignWaiterToRestaurant_Returns500_WhenServiceThrows()
    {
        // Arrange
        var request = new AssignWaiterToRestaurantRequest { UserId = "user123", RestaurantId = "restaurant1" };
        _waiterServiceMock.Setup(s => s.AssignWaiterToRestaurantAsync(request.RestaurantId, request.UserId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.AssignWaiterToRestaurant(request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    #endregion
}
