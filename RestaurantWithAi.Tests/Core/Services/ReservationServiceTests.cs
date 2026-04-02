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
    #region GetReservationsByGuestAsync

    [Fact]
    public async Task GetReservationsByGuestAsync_WhenRepositoryReturnsReservations_ReturnsMappedCollection()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var guestUserId = "guest-user-id";
        var reservations = new List<Reservation>
        {
            CreateReservation(guestUserId),
            CreateReservation(guestUserId)
        };

        repositoryMock
            .Setup(r => r.GetReservationsByGuestAsync(guestUserId))
            .ReturnsAsync(reservations);

        var sut = CreateSut(repositoryMock.Object);

        var result = (await sut.GetReservationsByGuestAsync(guestUserId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(guestUserId, r.GuestUserId));
        repositoryMock.Verify(r => r.GetReservationsByGuestAsync(guestUserId), Times.Once);
    }

    [Fact]
    public async Task GetReservationsByGuestAsync_WhenRepositoryReturnsEmpty_ReturnsEmptyCollection()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        repositoryMock
            .Setup(r => r.GetReservationsByGuestAsync(It.IsAny<string>()))
            .ReturnsAsync([]);

        var sut = CreateSut(repositoryMock.Object);

        var result = await sut.GetReservationsByGuestAsync("guest-id");

        Assert.Empty(result);
    }

    #endregion

    #region GetReservationsByWaiterAsync

    [Fact]
    public async Task GetReservationsByWaiterAsync_WhenRepositoryReturnsReservations_ReturnsMappedCollection()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var waiterId = "waiter-user-id";
        var reservations = new List<Reservation>
        {
            CreateReservation("guest1", assignedWaiterId: waiterId),
            CreateReservation("guest2", assignedWaiterId: waiterId)
        };

        repositoryMock
            .Setup(r => r.GetReservationsByWaiterAsync(waiterId))
            .ReturnsAsync(reservations);

        var sut = CreateSut(repositoryMock.Object);

        var result = (await sut.GetReservationsByWaiterAsync(waiterId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(waiterId, r.AssignedWaiterId));
        repositoryMock.Verify(r => r.GetReservationsByWaiterAsync(waiterId), Times.Once);
    }

    #endregion

    #region CreateReservationAsync

    [Fact]
    public async Task CreateReservationAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<IReservationRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.CreateReservationAsync(null!, "guest-id"));
        repositoryMock.Verify(r => r.CreateReservationAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenCalledByGuest_SetsGuestUserIdFromParameter()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var guestUserId = "guest-user-id";
        Reservation? capturedReservation = null;

        var reservation = CreateReservation(guestUserId);
        repositoryMock
            .Setup(r => r.CreateReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => capturedReservation = r)
            .ReturnsAsync(reservation);

        var sut = CreateSut(repositoryMock.Object);
        var request = new CreateReservationRequest
        {
            RestaurantId = Guid.NewGuid(),
            TableNumber = 1,
            GuestName = "John Doe",
            StartTime = DateTime.UtcNow.AddHours(1),
            DurationMinutes = 60,
            NumberOfGuests = 2
        };

        await sut.CreateReservationAsync(request, guestUserId);

        Assert.NotNull(capturedReservation);
        Assert.Equal(guestUserId, capturedReservation!.GuestUserId);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenCalledByAdmin_SetsGuestUserIdToNull()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        Reservation? capturedReservation = null;

        var reservation = CreateReservation(null);
        repositoryMock
            .Setup(r => r.CreateReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => capturedReservation = r)
            .ReturnsAsync(reservation);

        var sut = CreateSut(repositoryMock.Object);
        var request = new CreateReservationRequest
        {
            RestaurantId = Guid.NewGuid(),
            TableNumber = 1,
            GuestName = "Walk-in Guest",
            StartTime = DateTime.UtcNow.AddHours(1),
            DurationMinutes = 60,
            NumberOfGuests = 2
        };

        await sut.CreateReservationAsync(request, null);

        Assert.NotNull(capturedReservation);
        Assert.Null(capturedReservation!.GuestUserId);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenRepositoryThrowsConflict_PropagatesException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        repositoryMock
            .Setup(r => r.CreateReservationAsync(It.IsAny<Reservation>()))
            .ThrowsAsync(new ReservationConflictException("Table is not available."));

        var sut = CreateSut(repositoryMock.Object);
        var request = new CreateReservationRequest
        {
            RestaurantId = Guid.NewGuid(),
            TableNumber = 1,
            GuestName = "Test",
            StartTime = DateTime.UtcNow,
            DurationMinutes = 60,
            NumberOfGuests = 2
        };

        await Assert.ThrowsAsync<ReservationConflictException>(() => sut.CreateReservationAsync(request, "guest-id"));
    }

    #endregion

    #region DeleteReservationAsync

    [Fact]
    public async Task DeleteReservationAsync_WhenCalled_DelegatesToRepository()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservationId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.DeleteReservationAsync(reservationId))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.DeleteReservationAsync(reservationId);

        repositoryMock.Verify(r => r.DeleteReservationAsync(reservationId), Times.Once);
    }

    #endregion

    #region UpdateReservationTimeAsync

    [Fact]
    public async Task UpdateReservationTimeAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<IReservationRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateReservationTimeAsync(Guid.NewGuid(), null!));
        repositoryMock.Verify(r => r.UpdateReservationTimeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateReservationTimeAsync_WhenRequestIsValid_DelegatesToRepository()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var id = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(2);
        var durationMinutes = 90;

        repositoryMock
            .Setup(r => r.UpdateReservationTimeAsync(id, startTime, durationMinutes))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.UpdateReservationTimeAsync(id, new UpdateReservationTimeRequest { StartTime = startTime, DurationMinutes = durationMinutes });

        repositoryMock.Verify(r => r.UpdateReservationTimeAsync(id, startTime, durationMinutes), Times.Once);
    }

    #endregion

    #region UpdateReservationTableAsync

    [Fact]
    public async Task UpdateReservationTableAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<IReservationRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateReservationTableAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task UpdateReservationTableAsync_WhenRequestIsValid_DelegatesToRepository()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var id = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.UpdateReservationTableAsync(id, 3))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.UpdateReservationTableAsync(id, new UpdateReservationTableRequest { TableNumber = 3 });

        repositoryMock.Verify(r => r.UpdateReservationTableAsync(id, 3), Times.Once);
    }

    #endregion

    #region UpdateReservationWaiterAsync

    [Fact]
    public async Task UpdateReservationWaiterAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<IReservationRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateReservationWaiterAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task UpdateReservationWaiterAsync_WhenRequestIsValid_DelegatesToRepository()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var id = Guid.NewGuid();
        var waiterId = "waiter-id";

        repositoryMock
            .Setup(r => r.UpdateReservationWaiterAsync(id, waiterId))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.UpdateReservationWaiterAsync(id, new UpdateReservationWaiterRequest { WaiterId = waiterId });

        repositoryMock.Verify(r => r.UpdateReservationWaiterAsync(id, waiterId), Times.Once);
    }

    #endregion

    #region UpdateReservationStatusAsync

    [Fact]
    public async Task UpdateReservationStatusAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<IReservationRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateReservationStatusAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task UpdateReservationStatusAsync_WhenRequestIsValid_DelegatesToRepository()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var id = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.UpdateReservationStatusAsync(id, ReservationStatus.InProgress))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.UpdateReservationStatusAsync(id, new UpdateReservationStatusRequest { Status = ReservationStatus.InProgress });

        repositoryMock.Verify(r => r.UpdateReservationStatusAsync(id, ReservationStatus.InProgress), Times.Once);
    }

    #endregion

    private static ReservationService CreateSut(IReservationRepository repository)
    {
        var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<ReservationMappingProfile>());
        var mapper = mapperConfiguration.CreateMapper();
        return new ReservationService(repository, mapper);
    }

    private static Reservation CreateReservation(string? guestUserId, string? assignedWaiterId = null)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            TableNumber = 1,
            GuestUserId = guestUserId,
            GuestName = "Test Guest",
            AssignedWaiterId = assignedWaiterId,
            StartTime = DateTime.UtcNow.AddHours(1),
            DurationMinutes = 60,
            NumberOfGuests = 2,
            Status = ReservationStatus.Created
        };
    }
}
