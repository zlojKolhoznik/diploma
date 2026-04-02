using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Core.Mappings;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class ReservationMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void ReservationToReservationResponse_MapsAllFields()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var reservation = CreateReservation();

        var result = mapper.Map<ReservationResponse>(reservation);

        Assert.Equal(reservation.Id, result.Id);
        Assert.Equal(reservation.RestaurantId, result.RestaurantId);
        Assert.Equal(reservation.TableNumber, result.TableNumber);
        Assert.Equal(reservation.GuestId, result.GuestId);
        Assert.Equal(reservation.GuestName, result.GuestName);
        Assert.Equal(reservation.WaiterId, result.WaiterId);
        Assert.Equal(reservation.StartTime, result.StartTime);
        Assert.Equal(reservation.DurationMinutes, result.DurationMinutes);
        Assert.Equal(reservation.NumberOfGuests, result.NumberOfGuests);
        Assert.Equal(reservation.Status, result.Status);
    }

    [Fact]
    public void CreateReservationRequestToReservation_DoesNotMapIgnoredFields()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var request = new CreateReservationRequest
        {
            RestaurantId = Guid.NewGuid(),
            GuestId = "guest-123",
            GuestName = "John Doe",
            StartTime = DateTimeOffset.UtcNow,
            DurationMinutes = 60,
            NumberOfGuests = 2
        };

        var result = mapper.Map<Reservation>(request);

        Assert.NotEqual(default, result.Id);
        Assert.Equal(ReservationStatus.Created, result.Status);
        Assert.Null(result.TableNumber);
        Assert.Null(result.WaiterId);
        Assert.Equal(request.RestaurantId, result.RestaurantId);
        Assert.Equal(request.GuestId, result.GuestId);
        Assert.Equal(request.GuestName, result.GuestName);
        Assert.Equal(request.StartTime, result.StartTime);
        Assert.Equal(request.DurationMinutes, result.DurationMinutes);
        Assert.Equal(request.NumberOfGuests, result.NumberOfGuests);
    }

    private static Reservation CreateReservation()
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            TableNumber = 3,
            GuestId = "guest-abc",
            GuestName = "Jane Doe",
            WaiterId = "waiter-xyz",
            StartTime = DateTimeOffset.UtcNow,
            DurationMinutes = 90,
            NumberOfGuests = 4,
            Status = ReservationStatus.InProgress,
            Restaurant = new Restaurant { City = "Kyiv", Address = "Test" }
        };
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(cfg => cfg.AddProfile<ReservationMappingProfile>());
    }
}
