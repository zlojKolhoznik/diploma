using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantWithAi.Api.Controllers;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Tests;

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

    #region GetWaiters

    [Fact]
    public async Task GetWaiters_ReturnsOkWithWaiters_WhenWaitersExist()
    {
        // Arrange
        var waiters = new List<WaiterDto>
        {
            new() { UserId = "user-1", RestaurantId = 1 },
            new() { UserId = "user-2", RestaurantId = 2 }
        };
        _waiterServiceMock.Setup(s => s.GetAllWaitersAsync()).ReturnsAsync(waiters);

        // Act
        var result = await _controller.GetWaiters();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedWaiters = Assert.IsAssignableFrom<IEnumerable<WaiterDto>>(okResult.Value);
        Assert.Equal(2, returnedWaiters.Count());
    }

    [Fact]
    public async Task GetWaiters_ReturnsOkWithEmptyList_WhenNoWaitersExist()
    {
        // Arrange
        _waiterServiceMock.Setup(s => s.GetAllWaitersAsync()).ReturnsAsync([]);

        // Act
        var result = await _controller.GetWaiters();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedWaiters = Assert.IsAssignableFrom<IEnumerable<WaiterDto>>(okResult.Value);
        Assert.Empty(returnedWaiters);
    }

    [Fact]
    public async Task GetWaiters_Returns500_WhenExceptionThrown()
    {
        // Arrange
        _waiterServiceMock.Setup(s => s.GetAllWaitersAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetWaiters();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        Assert.NotNull(statusResult.Value);
    }

    #endregion

    #region AssignWaiterRole

    [Fact]
    public async Task AssignWaiterRole_ReturnsCreated_WhenSuccessful()
    {
        // Arrange
        const string userId = "user-123";
        var request = new AssignWaiterRequest { RestaurantId = 5 };
        var waiterDto = new WaiterDto { UserId = userId, RestaurantId = 5 };
        _waiterServiceMock.Setup(s => s.AssignWaiterRoleAsync(userId, 5)).ReturnsAsync(waiterDto);

        // Act
        var result = await _controller.AssignWaiterRole(userId, request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedWaiter = Assert.IsType<WaiterDto>(createdResult.Value);
        Assert.Equal(userId, returnedWaiter.UserId);
        Assert.Equal(5, returnedWaiter.RestaurantId);
    }

    [Fact]
    public async Task AssignWaiterRole_Returns500_WhenExceptionThrown()
    {
        // Arrange
        const string userId = "user-123";
        var request = new AssignWaiterRequest { RestaurantId = 5 };
        _waiterServiceMock.Setup(s => s.AssignWaiterRoleAsync(userId, 5)).ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.AssignWaiterRole(userId, request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        Assert.NotNull(statusResult.Value);
    }

    #endregion

    #region AssignWaiterToRestaurant

    [Fact]
    public async Task AssignWaiterToRestaurant_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        const int restaurantId = 10;
        var request = new AssignToRestaurantRequest { UserId = "user-456" };
        var waiterDto = new WaiterDto { UserId = "user-456", RestaurantId = restaurantId };
        _waiterServiceMock.Setup(s => s.AssignWaiterToRestaurantAsync("user-456", restaurantId)).ReturnsAsync(waiterDto);

        // Act
        var result = await _controller.AssignWaiterToRestaurant(restaurantId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedWaiter = Assert.IsType<WaiterDto>(okResult.Value);
        Assert.Equal("user-456", returnedWaiter.UserId);
        Assert.Equal(restaurantId, returnedWaiter.RestaurantId);
    }

    [Fact]
    public async Task AssignWaiterToRestaurant_ReturnsNotFound_WhenWaiterNotFound()
    {
        // Arrange
        const int restaurantId = 10;
        var request = new AssignToRestaurantRequest { UserId = "nonexistent-user" };
        _waiterServiceMock
            .Setup(s => s.AssignWaiterToRestaurantAsync("nonexistent-user", restaurantId))
            .ThrowsAsync(new WaiterNotFoundException("Waiter with user ID 'nonexistent-user' was not found."));

        // Act
        var result = await _controller.AssignWaiterToRestaurant(restaurantId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task AssignWaiterToRestaurant_Returns500_WhenExceptionThrown()
    {
        // Arrange
        const int restaurantId = 10;
        var request = new AssignToRestaurantRequest { UserId = "user-456" };
        _waiterServiceMock
            .Setup(s => s.AssignWaiterToRestaurantAsync("user-456", restaurantId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.AssignWaiterToRestaurant(restaurantId, request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        Assert.NotNull(statusResult.Value);
    }

    #endregion
}
