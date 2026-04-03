using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Moq;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class ReservationServiceTests
{
    [Fact]
    public async Task CreateReservationAsync_WhenGuestInitiates_SetsGuestIdFromCurrentUser()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var restaurantId = Guid.NewGuid();
        Reservation? capturedReservation = null;

        repositoryMock.Setup(r => r.RestaurantExistsAsync(restaurantId)).ReturnsAsync(true);
        repositoryMock
            .Setup(r => r.AddReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(reservation => capturedReservation = reservation)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.CreateReservationAsync(new CreateReservationRequest
        {
            RestaurantId = restaurantId,
            StartTime = new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc),
            ApproximateDurationMinutes = 60,
            NumberOfGuests = 2
        }, "guest-1", isAdminInitiated: false);

        Assert.NotNull(capturedReservation);
        Assert.Equal("guest-1", capturedReservation!.GuestId);
        Assert.Equal(ReservationStatuses.Created, capturedReservation.Status);
    }

    [Theory]
    [InlineData(8, 59)]
    [InlineData(21, 1)]
    public async Task CreateReservationAsync_WhenOutsideWorkingHours_ThrowsArgumentException(int hour, int minute)
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock.Setup(r => r.RestaurantExistsAsync(restaurantId)).ReturnsAsync(true);

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.CreateReservationAsync(new CreateReservationRequest
        {
            RestaurantId = restaurantId,
            StartTime = new DateTime(2026, 4, 2, hour, minute, 0, DateTimeKind.Utc),
            ApproximateDurationMinutes = 60,
            NumberOfGuests = 2
        }, "guest-1", isAdminInitiated: false));
    }

    [Theory]
    [InlineData(9, 0)]
    [InlineData(21, 0)]
    public async Task CreateReservationAsync_WhenAtBoundaryWorkingHours_AllowsCreation(int hour, int minute)
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock.Setup(r => r.RestaurantExistsAsync(restaurantId)).ReturnsAsync(true);
        repositoryMock.Setup(r => r.AddReservationAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.CreateReservationAsync(new CreateReservationRequest
        {
            RestaurantId = restaurantId,
            StartTime = new DateTime(2026, 4, 2, hour, minute, 0, DateTimeKind.Utc),
            ApproximateDurationMinutes = 60,
            NumberOfGuests = 2
        }, "guest-1", isAdminInitiated: false);

        repositoryMock.Verify(r => r.AddReservationAsync(It.IsAny<Reservation>()), Times.Once);
    }

    [Fact]
    public async Task DeleteReservationAsync_WhenGuestIsNotOwner_ThrowsUnauthorizedAccessException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservationId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservationId))
            .ReturnsAsync(new Reservation
            {
                Id = reservationId,
                RestaurantId = Guid.NewGuid(),
                GuestId = "guest-owner",
                StartTime = DateTime.UtcNow,
                ApproximateDurationMinutes = 60,
                NumberOfGuests = 3,
                Status = ReservationStatuses.Created
            });

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.DeleteReservationAsync(reservationId, "guest-other", isAdmin: false));
    }

    [Fact]
    public async Task UpdateReservationTableAsync_WhenConflictExists_ThrowsReservationConflictException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservationId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservationId))
            .ReturnsAsync(new Reservation
            {
                Id = reservationId,
                RestaurantId = restaurantId,
                StartTime = DateTime.UtcNow,
                ApproximateDurationMinutes = 60,
                NumberOfGuests = 4,
                Status = ReservationStatuses.Created
            });
        repositoryMock.Setup(r => r.TableExistsAsync(restaurantId, 7)).ReturnsAsync(true);
        repositoryMock
            .Setup(r => r.HasTableConflictAsync(restaurantId, 7, It.IsAny<DateTime>(), 60, reservationId))
            .ReturnsAsync(true);

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ReservationConflictException>(() =>
            sut.UpdateReservationTableAsync(reservationId, new UpdateReservationTableRequest { TableNumber = 7 }));
    }

    [Fact]
    public async Task UpdateReservationStatusAsync_WhenSkippingStep_ThrowsInvalidReservationStatusTransitionException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservationId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservationId))
            .ReturnsAsync(new Reservation
            {
                Id = reservationId,
                RestaurantId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                ApproximateDurationMinutes = 60,
                NumberOfGuests = 2,
                Status = ReservationStatuses.Created
            });

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<InvalidReservationStatusTransitionException>(() =>
            sut.UpdateReservationStatusAsync(reservationId, new UpdateReservationStatusRequest
            {
                Status = ReservationStatuses.PendingPayment
            }));
    }

    private static ReservationService CreateSut(IReservationRepository repository)
    {
        var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<ReservationMappingProfile>());
        var mapper = mapperConfiguration.CreateMapper();
        return new ReservationService(repository, mapper);
    }
}


