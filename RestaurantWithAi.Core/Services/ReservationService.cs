using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Core.Services;

public class ReservationService(IReservationRepository reservationRepository, IMapper mapper) : IReservationsService
{
    public async Task<IEnumerable<ReservationResponse>> GetReservationsForGuestAsync(string guestId)
    {
        var reservations = await reservationRepository.GetReservationsByGuestIdAsync(guestId);
        return mapper.Map<IEnumerable<ReservationResponse>>(reservations);
    }

    public async Task<IEnumerable<ReservationResponse>> GetReservationsForWaiterAsync(string waiterId)
    {
        var reservations = await reservationRepository.GetReservationsByWaiterIdAsync(waiterId);
        return mapper.Map<IEnumerable<ReservationResponse>>(reservations);
    }

    public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request, string? guestId)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservation = mapper.Map<Reservation>(request);

        if (!string.IsNullOrEmpty(guestId))
        {
            reservation.GuestId = guestId;
        }

        var created = await reservationRepository.AddReservationAsync(reservation);
        return mapper.Map<ReservationResponse>(created);
    }

    public async Task CancelReservationAsync(Guid id, string? guestId, bool isAdmin)
    {
        var reservation = await reservationRepository.GetReservationByIdAsync(id);

        if (!isAdmin && reservation.GuestId != guestId)
            throw new UnauthorizedAccessException("You do not have access to this reservation.");

        if (reservation.Status == ReservationStatus.Closed || reservation.Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("The reservation cannot be cancelled in its current state.");

        reservation.Status = ReservationStatus.Cancelled;
        await reservationRepository.UpdateReservationAsync(reservation);
    }

    public async Task UpdateReservationTimeAsync(Guid id, UpdateReservationTimeRequest request, string? guestId, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservation = await reservationRepository.GetReservationByIdAsync(id);

        if (!isAdmin && reservation.GuestId != guestId)
            throw new UnauthorizedAccessException("You do not have access to this reservation.");

        if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Closed)
            throw new InvalidOperationException("Cannot update time for a closed or cancelled reservation.");

        if (reservation.TableNumber.HasValue)
        {
            var hasOverlap = await reservationRepository.HasOverlappingReservationAsync(
                reservation.RestaurantId,
                reservation.TableNumber.Value,
                request.StartTime,
                request.DurationMinutes,
                id);

            if (hasOverlap)
                throw new InvalidOperationException("The new time overlaps with an existing reservation on the same table.");
        }

        reservation.StartTime = request.StartTime;
        reservation.DurationMinutes = request.DurationMinutes;
        await reservationRepository.UpdateReservationAsync(reservation);
    }

    public async Task UpdateReservationTableAsync(Guid id, UpdateReservationTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservation = await reservationRepository.GetReservationByIdAsync(id);

        if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Closed)
            throw new InvalidOperationException("Cannot update table for a closed or cancelled reservation.");

        var hasOverlap = await reservationRepository.HasOverlappingReservationAsync(
            reservation.RestaurantId,
            request.TableNumber,
            reservation.StartTime,
            reservation.DurationMinutes,
            id);

        if (hasOverlap)
            throw new InvalidOperationException("The table has an overlapping reservation at the requested time.");

        reservation.TableNumber = request.TableNumber;
        await reservationRepository.UpdateReservationAsync(reservation);
    }

    public async Task UpdateReservationWaiterAsync(Guid id, UpdateReservationWaiterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservation = await reservationRepository.GetReservationByIdAsync(id);

        if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Closed)
            throw new InvalidOperationException("Cannot update waiter for a closed or cancelled reservation.");

        reservation.WaiterId = request.WaiterId;
        await reservationRepository.UpdateReservationAsync(reservation);
    }

    private static readonly IReadOnlyDictionary<ReservationStatus, ReservationStatus> ValidStatusTransitions =
        new Dictionary<ReservationStatus, ReservationStatus>
        {
            { ReservationStatus.Created, ReservationStatus.InProgress },
            { ReservationStatus.InProgress, ReservationStatus.PendingPayment },
            { ReservationStatus.PendingPayment, ReservationStatus.Closed }
        };

    public async Task UpdateReservationStatusAsync(Guid id, UpdateReservationStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservation = await reservationRepository.GetReservationByIdAsync(id);

        if (!ValidStatusTransitions.TryGetValue(reservation.Status, out var allowedNext) || allowedNext != request.Status)
            throw new InvalidOperationException(
                $"Cannot transition from {reservation.Status} to {request.Status}. " +
                $"Expected next status: {(ValidStatusTransitions.TryGetValue(reservation.Status, out var next) ? next : "none")}.");

        reservation.Status = request.Status;
        await reservationRepository.UpdateReservationAsync(reservation);
    }
}
