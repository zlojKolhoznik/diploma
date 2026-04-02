using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Moq;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Orders;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrderAsync_WhenReservationClosed_ThrowsInvalidOperationException()
    {
        var repositoryMock = new Mock<IOrderRepository>();
        var restaurantId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetReservationForScopeAsync(restaurantId, reservationId))
            .ReturnsAsync(new Reservation
            {
                Id = reservationId,
                RestaurantId = restaurantId,
                Status = ReservationStatuses.Closed
            });

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreateOrderAsync(restaurantId, reservationId, new CreateOrderRequest(), "waiter-1", isAdmin: false));
    }

    [Fact]
    public async Task CreateOrderAsync_WhenAnotherOpenOrderExists_ThrowsInvalidOperationException()
    {
        var repositoryMock = new Mock<IOrderRepository>();
        var restaurantId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetReservationForScopeAsync(restaurantId, reservationId))
            .ReturnsAsync(new Reservation
            {
                Id = reservationId,
                RestaurantId = restaurantId,
                Status = ReservationStatuses.Created,
                AssignedWaiterId = "waiter-1"
            });
        repositoryMock.Setup(r => r.HasOpenOrderForReservationAsync(restaurantId, reservationId)).ReturnsAsync(true);

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreateOrderAsync(restaurantId, reservationId, new CreateOrderRequest(), "waiter-1", isAdmin: false));
    }

    [Fact]
    public async Task AddOrderItemAsync_WhenDishUnavailable_ThrowsKeyNotFoundException()
    {
        var repositoryMock = new Mock<IOrderRepository>();
        var restaurantId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var dishId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetReservationForScopeAsync(restaurantId, reservationId))
            .ReturnsAsync(new Reservation
            {
                Id = reservationId,
                RestaurantId = restaurantId,
                Status = ReservationStatuses.Created,
                AssignedWaiterId = "waiter-1"
            });
        repositoryMock
            .Setup(r => r.GetOrderByIdAsync(restaurantId, reservationId, orderId))
            .ReturnsAsync(new Order
            {
                Id = orderId,
                RestaurantId = restaurantId,
                ReservationId = reservationId,
                Status = OrderStatuses.Created
            });
        repositoryMock.Setup(r => r.IsDishAvailableAtRestaurantAsync(restaurantId, dishId)).ReturnsAsync(false);

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.AddOrderItemAsync(restaurantId, reservationId, orderId, new AddOrderItemRequest
            {
                DishId = dishId,
                Quantity = 1
            }, "waiter-1", isAdmin: false));
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WhenStatusSkipsFlow_ThrowsInvalidOperationException()
    {
        var repositoryMock = new Mock<IOrderRepository>();
        var restaurantId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetReservationForScopeAsync(restaurantId, reservationId))
            .ReturnsAsync(new Reservation
            {
                Id = reservationId,
                RestaurantId = restaurantId,
                Status = ReservationStatuses.Created,
                AssignedWaiterId = "waiter-1"
            });
        repositoryMock
            .Setup(r => r.GetOrderByIdAsync(restaurantId, reservationId, orderId))
            .ReturnsAsync(new Order
            {
                Id = orderId,
                RestaurantId = restaurantId,
                ReservationId = reservationId,
                Status = OrderStatuses.Created
            });

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.UpdateOrderStatusAsync(restaurantId, reservationId, orderId, new UpdateOrderStatusRequest
            {
                Status = OrderStatuses.Served
            }, "waiter-1", isAdmin: false));
    }

    [Fact]
    public async Task CreateOrderAsync_WhenWaiterNotAssigned_ThrowsUnauthorizedAccessException()
    {
        var repositoryMock = new Mock<IOrderRepository>();
        var restaurantId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetReservationForScopeAsync(restaurantId, reservationId))
            .ReturnsAsync(new Reservation
            {
                Id = reservationId,
                RestaurantId = restaurantId,
                Status = ReservationStatuses.Created,
                AssignedWaiterId = "waiter-owner"
            });

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.CreateOrderAsync(restaurantId, reservationId, new CreateOrderRequest(), "waiter-other", isAdmin: false));
    }

    private static OrderService CreateSut(IOrderRepository repository)
    {
        var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<OrderMappingProfile>());
        var mapper = mapperConfiguration.CreateMapper();
        return new OrderService(repository, mapper);
    }
}
