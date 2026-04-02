using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Moq;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class ReservationServiceTests
{
    #region GetReservationsForGuestAsync

    [Fact]
    public async Task GetReservationsForGuestAsync_WhenRepositoryReturnsReservations_ReturnsMappedCollection()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var guestId = "guest-123";
        var reservations = new List<Reservation>
        {
            CreateReservation(guestId: guestId),
            CreateReservation(guestId: guestId)
        };

        repositoryMock
            .Setup(r => r.GetReservationsByGuestIdAsync(guestId))
            .ReturnsAsync(reservations);

        var sut = CreateSut(repositoryMock.Object);

        var result = (await sut.GetReservationsForGuestAsync(guestId)).ToList();

        Assert.Equal(2, result.Count);
        repositoryMock.Verify(r => r.GetReservationsByGuestIdAsync(guestId), Times.Once);
    }

    #endregion

    #region GetReservationsForWaiterAsync

    [Fact]
    public async Task GetReservationsForWaiterAsync_WhenRepositoryReturnsReservations_ReturnsMappedCollection()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var waiterId = "waiter-abc";
        var reservations = new List<Reservation>
        {
            CreateReservation(waiterId: waiterId),
            CreateReservation(waiterId: waiterId)
        };

        repositoryMock
            .Setup(r => r.GetReservationsByWaiterIdAsync(waiterId))
            .ReturnsAsync(reservations);

        var sut = CreateSut(repositoryMock.Object);

        var result = (await sut.GetReservationsForWaiterAsync(waiterId)).ToList();

        Assert.Equal(2, result.Count);
        repositoryMock.Verify(r => r.GetReservationsByWaiterIdAsync(waiterId), Times.Once);
    }

    #endregion

    #region CreateReservationAsync

    [Fact]
    public async Task CreateReservationAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<IReservationRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.CreateReservationAsync(null!, null));
    }

    [Fact]
    public async Task CreateReservationAsync_WhenGuestIdProvided_SetsGuestIdFromParameter()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var guestId = "guest-456";
        Reservation? capturedReservation = null;

        repositoryMock
            .Setup(r => r.AddReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(res => capturedReservation = res)
            .ReturnsAsync((Reservation res) => res);

        var sut = CreateSut(repositoryMock.Object);
        var request = CreateRequest();

        await sut.CreateReservationAsync(request, guestId);

        Assert.NotNull(capturedReservation);
        Assert.Equal(guestId, capturedReservation!.GuestId);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenGuestIdIsNull_UsesRequestGuestName()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        Reservation? capturedReservation = null;

        repositoryMock
            .Setup(r => r.AddReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(res => capturedReservation = res)
            .ReturnsAsync((Reservation res) => res);

        var sut = CreateSut(repositoryMock.Object);
        var request = CreateRequest(guestName: "John Admin");

        await sut.CreateReservationAsync(request, null);

        Assert.NotNull(capturedReservation);
        Assert.Null(capturedReservation!.GuestId);
        Assert.Equal("John Admin", capturedReservation.GuestName);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenRequestIsValid_ReturnsMappedResponse()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var restaurantId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.AddReservationAsync(It.IsAny<Reservation>()))
            .ReturnsAsync((Reservation res) => res);

        var sut = CreateSut(repositoryMock.Object);
        var request = CreateRequest(restaurantId: restaurantId, durationMinutes: 60, numberOfGuests: 4);

        var result = await sut.CreateReservationAsync(request, "guest-1");

        Assert.Equal(restaurantId, result.RestaurantId);
        Assert.Equal(60, result.DurationMinutes);
        Assert.Equal(4, result.NumberOfGuests);
        Assert.Equal(ReservationStatus.Created, result.Status);
    }

    #endregion

    #region CancelReservationAsync

    [Fact]
    public async Task CancelReservationAsync_WhenReservationNotFound_ThrowsKeyNotFoundException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var id = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Reservation with ID {id} not found."));

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.CancelReservationAsync(id, "guest-1", false));
    }

    [Fact]
    public async Task CancelReservationAsync_WhenGuestTriesToCancelOthersReservation_ThrowsUnauthorizedAccessException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation(guestId: "guest-owner");

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => sut.CancelReservationAsync(reservation.Id, "different-guest", false));
    }

    [Fact]
    public async Task CancelReservationAsync_WhenReservationIsClosed_ThrowsInvalidOperationException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation(status: ReservationStatus.Closed);

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.CancelReservationAsync(reservation.Id, null, true));
    }

    [Fact]
    public async Task CancelReservationAsync_WhenAdminCancels_SetsCancelledStatus()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation(status: ReservationStatus.Created);
        Reservation? updatedReservation = null;

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);
        repositoryMock
            .Setup(r => r.UpdateReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => updatedReservation = r)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.CancelReservationAsync(reservation.Id, null, true);

        Assert.Equal(ReservationStatus.Cancelled, updatedReservation!.Status);
    }

    [Fact]
    public async Task CancelReservationAsync_WhenGuestCancelsOwnReservation_SetsCancelledStatus()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var guestId = "guest-abc";
        var reservation = CreateReservation(guestId: guestId, status: ReservationStatus.Created);
        Reservation? updatedReservation = null;

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);
        repositoryMock
            .Setup(r => r.UpdateReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => updatedReservation = r)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.CancelReservationAsync(reservation.Id, guestId, false);

        Assert.Equal(ReservationStatus.Cancelled, updatedReservation!.Status);
    }

    #endregion

    #region UpdateReservationTimeAsync

    [Fact]
    public async Task UpdateReservationTimeAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<IReservationRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.UpdateReservationTimeAsync(Guid.NewGuid(), null!, null, true));
    }

    [Fact]
    public async Task UpdateReservationTimeAsync_WhenGuestTriesToUpdateOthersReservation_ThrowsUnauthorizedAccessException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation(guestId: "guest-owner");

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        var sut = CreateSut(repositoryMock.Object);
        var request = new UpdateReservationTimeRequest { StartTime = DateTimeOffset.UtcNow, DurationMinutes = 60 };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => sut.UpdateReservationTimeAsync(reservation.Id, request, "different-guest", false));
    }

    [Fact]
    public async Task UpdateReservationTimeAsync_WhenTableHasOverlap_ThrowsInvalidOperationException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation(tableNumber: 2);

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);
        repositoryMock
            .Setup(r => r.HasOverlappingReservationAsync(
                reservation.RestaurantId, 2, It.IsAny<DateTimeOffset>(), It.IsAny<int>(), reservation.Id))
            .ReturnsAsync(true);

        var sut = CreateSut(repositoryMock.Object);
        var request = new UpdateReservationTimeRequest { StartTime = DateTimeOffset.UtcNow.AddHours(1), DurationMinutes = 60 };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.UpdateReservationTimeAsync(reservation.Id, request, null, true));

        Assert.Contains("overlaps", ex.Message);
    }

    [Fact]
    public async Task UpdateReservationTimeAsync_WhenNoOverlap_UpdatesReservation()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation(tableNumber: 1);
        Reservation? updatedReservation = null;

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);
        repositoryMock
            .Setup(r => r.HasOverlappingReservationAsync(
                reservation.RestaurantId, 1, It.IsAny<DateTimeOffset>(), It.IsAny<int>(), reservation.Id))
            .ReturnsAsync(false);
        repositoryMock
            .Setup(r => r.UpdateReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => updatedReservation = r)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);
        var newStart = DateTimeOffset.UtcNow.AddHours(2);
        var request = new UpdateReservationTimeRequest { StartTime = newStart, DurationMinutes = 45 };

        await sut.UpdateReservationTimeAsync(reservation.Id, request, null, true);

        Assert.Equal(newStart, updatedReservation!.StartTime);
        Assert.Equal(45, updatedReservation.DurationMinutes);
    }

    #endregion

    #region UpdateReservationTableAsync

    [Fact]
    public async Task UpdateReservationTableAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<IReservationRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.UpdateReservationTableAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task UpdateReservationTableAsync_WhenTableHasOverlap_ThrowsInvalidOperationException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation();

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);
        repositoryMock
            .Setup(r => r.HasOverlappingReservationAsync(
                reservation.RestaurantId, 5, reservation.StartTime, reservation.DurationMinutes, reservation.Id))
            .ReturnsAsync(true);

        var sut = CreateSut(repositoryMock.Object);
        var request = new UpdateReservationTableRequest { TableNumber = 5 };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.UpdateReservationTableAsync(reservation.Id, request));

        Assert.Contains("overlapping", ex.Message);
    }

    [Fact]
    public async Task UpdateReservationTableAsync_WhenNoOverlap_UpdatesReservation()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation();
        Reservation? updatedReservation = null;

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);
        repositoryMock
            .Setup(r => r.HasOverlappingReservationAsync(
                reservation.RestaurantId, 7, reservation.StartTime, reservation.DurationMinutes, reservation.Id))
            .ReturnsAsync(false);
        repositoryMock
            .Setup(r => r.UpdateReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => updatedReservation = r)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);
        var request = new UpdateReservationTableRequest { TableNumber = 7 };

        await sut.UpdateReservationTableAsync(reservation.Id, request);

        Assert.Equal(7, updatedReservation!.TableNumber);
    }

    #endregion

    #region UpdateReservationWaiterAsync

    [Fact]
    public async Task UpdateReservationWaiterAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<IReservationRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.UpdateReservationWaiterAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task UpdateReservationWaiterAsync_WhenReservationIsClosed_ThrowsInvalidOperationException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation(status: ReservationStatus.Closed);

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        var sut = CreateSut(repositoryMock.Object);
        var request = new UpdateReservationWaiterRequest { WaiterId = "waiter-1" };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.UpdateReservationWaiterAsync(reservation.Id, request));
    }

    [Fact]
    public async Task UpdateReservationWaiterAsync_WhenValid_UpdatesWaiter()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation(status: ReservationStatus.Created);
        Reservation? updatedReservation = null;

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);
        repositoryMock
            .Setup(r => r.UpdateReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => updatedReservation = r)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);
        var request = new UpdateReservationWaiterRequest { WaiterId = "waiter-99" };

        await sut.UpdateReservationWaiterAsync(reservation.Id, request);

        Assert.Equal("waiter-99", updatedReservation!.WaiterId);
    }

    #endregion

    #region UpdateReservationStatusAsync

    [Fact]
    public async Task UpdateReservationStatusAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var repositoryMock = new Mock<IReservationRepository>(MockBehavior.Strict);
        var sut = CreateSut(repositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.UpdateReservationStatusAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task UpdateReservationStatusAsync_WhenTransitionIsValid_UpdatesStatus()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation(status: ReservationStatus.Created);
        Reservation? updatedReservation = null;

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);
        repositoryMock
            .Setup(r => r.UpdateReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => updatedReservation = r)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);
        var request = new UpdateReservationStatusRequest { Status = ReservationStatus.InProgress };

        await sut.UpdateReservationStatusAsync(reservation.Id, request);

        Assert.Equal(ReservationStatus.InProgress, updatedReservation!.Status);
    }

    [Fact]
    public async Task UpdateReservationStatusAsync_WhenTransitionIsInvalid_ThrowsInvalidOperationException()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation(status: ReservationStatus.Created);

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        var sut = CreateSut(repositoryMock.Object);
        var request = new UpdateReservationStatusRequest { Status = ReservationStatus.Closed };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.UpdateReservationStatusAsync(reservation.Id, request));
    }

    [Fact]
    public async Task UpdateReservationStatusAsync_FullFlow_AllTransitionsSucceed()
    {
        var repositoryMock = new Mock<IReservationRepository>();
        var reservation = CreateReservation(status: ReservationStatus.Created);

        repositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);
        repositoryMock
            .Setup(r => r.UpdateReservationAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => reservation.Status = r.Status)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repositoryMock.Object);

        await sut.UpdateReservationStatusAsync(reservation.Id, new UpdateReservationStatusRequest { Status = ReservationStatus.InProgress });
        Assert.Equal(ReservationStatus.InProgress, reservation.Status);

        await sut.UpdateReservationStatusAsync(reservation.Id, new UpdateReservationStatusRequest { Status = ReservationStatus.PendingPayment });
        Assert.Equal(ReservationStatus.PendingPayment, reservation.Status);

        await sut.UpdateReservationStatusAsync(reservation.Id, new UpdateReservationStatusRequest { Status = ReservationStatus.Closed });
        Assert.Equal(ReservationStatus.Closed, reservation.Status);
    }

    #endregion

    private static ReservationService CreateSut(IReservationRepository repository)
    {
        var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<ReservationMappingProfile>());
        var mapper = mapperConfiguration.CreateMapper();
        return new ReservationService(repository, mapper);
    }

    private static Reservation CreateReservation(
        string? guestId = null,
        string? waiterId = null,
        int? tableNumber = null,
        ReservationStatus status = ReservationStatus.Created)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            GuestId = guestId,
            WaiterId = waiterId,
            TableNumber = tableNumber,
            StartTime = DateTimeOffset.UtcNow.AddHours(2),
            DurationMinutes = 60,
            NumberOfGuests = 2,
            Status = status,
            Restaurant = new Restaurant { City = "Kyiv", Address = "Test" }
        };
    }

    private static CreateReservationRequest CreateRequest(
        Guid? restaurantId = null,
        string? guestName = null,
        int durationMinutes = 60,
        int numberOfGuests = 2)
    {
        return new CreateReservationRequest
        {
            RestaurantId = restaurantId ?? Guid.NewGuid(),
            GuestName = guestName,
            StartTime = DateTimeOffset.UtcNow.AddHours(1),
            DurationMinutes = durationMinutes,
            NumberOfGuests = numberOfGuests
        };
    }
}
