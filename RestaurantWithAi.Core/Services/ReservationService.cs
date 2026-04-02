using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Core.Services;

public class ReservationService(IReservationRepository reservationRepository, IMapper mapper) : IReservationsService
{
    public async Task<IEnumerable<ReservationResponse>> GetReservationsByGuestAsync(string guestUserId)
    {
        var reservations = await reservationRepository.GetReservationsByGuestAsync(guestUserId);
        return mapper.Map<IEnumerable<ReservationResponse>>(reservations);
    }

    public async Task<IEnumerable<ReservationResponse>> GetReservationsByWaiterAsync(string waiterId)
    {
        var reservations = await reservationRepository.GetReservationsByWaiterAsync(waiterId);
        return mapper.Map<IEnumerable<ReservationResponse>>(reservations);
    }

    public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request, string? guestUserId)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservation = mapper.Map<Reservation>(request);
        reservation.GuestUserId = guestUserId;
        reservation.GuestName = request.GuestName ?? string.Empty;

        var created = await reservationRepository.CreateReservationAsync(reservation);
        return mapper.Map<ReservationResponse>(created);
    }

    public Task DeleteReservationAsync(Guid id)
    {
        return reservationRepository.DeleteReservationAsync(id);
    }

    public Task UpdateReservationTimeAsync(Guid id, UpdateReservationTimeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return reservationRepository.UpdateReservationTimeAsync(id, request.StartTime, request.DurationMinutes);
    }

    public Task UpdateReservationTableAsync(Guid id, UpdateReservationTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return reservationRepository.UpdateReservationTableAsync(id, request.TableNumber);
    }

    public Task UpdateReservationWaiterAsync(Guid id, UpdateReservationWaiterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return reservationRepository.UpdateReservationWaiterAsync(id, request.WaiterId);
    }

    public Task UpdateReservationStatusAsync(Guid id, UpdateReservationStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return reservationRepository.UpdateReservationStatusAsync(id, request.Status);
    }
}
