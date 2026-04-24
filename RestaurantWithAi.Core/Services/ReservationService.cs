using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Core.Services;

public class ReservationService(IReservationRepository reservationRepository, IMapper mapper) : IReservationsService
{
    private const int ReservationGapMinutes = 15;
    private const int MinReservationDurationMinutes = 30;
    private const int MaxReservationDurationMinutes = 600;
    private static readonly TimeOnly OpeningTime = new(9, 0);
    private static readonly TimeOnly ClosingTime = new(21, 0);

    public async Task<IEnumerable<ReservationResponse>> GetReservationsForGuestAsync(string guestId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guestId);
        var reservations = await reservationRepository.GetReservationsForGuestAsync(guestId);
        return mapper.Map<IEnumerable<ReservationResponse>>(reservations);
    }

    public async Task<IEnumerable<ReservationResponse>> GetReservationsForWaiterAsync(string waiterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(waiterId);
        var reservations = await reservationRepository.GetReservationsForWaiterAsync(waiterId);
        return mapper.Map<IEnumerable<ReservationResponse>>(reservations);
    }

    public async Task<IEnumerable<ReservationResponse>> GetAllReservationsAsync()
    {
        var reservations = await reservationRepository.GetAllReservationsAsync();
        return mapper.Map<IEnumerable<ReservationResponse>>(reservations);
    }

    public async Task CreateReservationAsync(CreateReservationRequest request, string currentUserId, bool isAdminInitiated)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RestaurantId == Guid.Empty)
            throw new ArgumentException("RestaurantId is required.", nameof(request));

        if (!await reservationRepository.RestaurantExistsAsync(request.RestaurantId))
            throw new KeyNotFoundException($"Restaurant with ID {request.RestaurantId} not found");

        ValidateCreationStartTimeWindow(request.StartTime);
        ValidateCreationLeadTime(request.StartTime);
        ValidateDurationMinutes(request.ApproximateDurationMinutes);

        var reservation = mapper.Map<Reservation>(request);
        reservation.Status = ReservationStatuses.Created;
        reservation.StartTime = EnsureUtc(reservation.StartTime);

        if (isAdminInitiated)
        {
            if (string.IsNullOrWhiteSpace(request.GuestName))
                throw new ArgumentException("GuestName is required when reservation is created by admin.", nameof(request));

            reservation.GuestName = request.GuestName.Trim();
            reservation.GuestId = string.IsNullOrWhiteSpace(request.GuestId) ? null : request.GuestId.Trim();
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(currentUserId);

            if (!string.IsNullOrWhiteSpace(request.GuestId) &&
                !string.Equals(request.GuestId, currentUserId, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("Guests can only create reservations for themselves.");
            }

            reservation.GuestId = currentUserId;
            reservation.GuestName = string.IsNullOrWhiteSpace(request.GuestName) ? null : request.GuestName.Trim();

            // If the current user is a waiter, their restaurant scope must match the reservation restaurant.
            var waiterRestaurantId = await reservationRepository.GetWaiterRestaurantIdAsync(currentUserId);
            if (waiterRestaurantId.HasValue && waiterRestaurantId.Value != request.RestaurantId)
                throw new UnauthorizedAccessException("Waiters can only create reservations for their assigned restaurant.");
        }

        await reservationRepository.AddReservationAsync(reservation);
    }

    public async Task DeleteReservationAsync(Guid id, string? currentUserId, bool isAdmin)
    {
        var reservation = await reservationRepository.GetReservationByIdAsync(id);

        EnsureOpenReservation(reservation);
        EnsureCancellationAllowed(reservation);

        if (!isAdmin)
            EnsureGuestOwnership(reservation, currentUserId);

        await reservationRepository.DeleteReservationAsync(id);
    }

    public async Task UpdateReservationTimeAsync(Guid id, UpdateReservationTimeRequest request, string? currentUserId, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservation = await reservationRepository.GetReservationByIdAsync(id);
        EnsureOpenReservation(reservation);

        if (!isAdmin)
            EnsureGuestOwnership(reservation, currentUserId);

        if (reservation.TableNumber.HasValue)
        {
            var hasConflict = await reservationRepository.HasTableConflictAsync(
                reservation.RestaurantId,
                reservation.TableNumber.Value,
                EnsureUtc(request.StartTime),
                request.ApproximateDurationMinutes,
                reservation.Id);

            if (hasConflict)
            {
                throw new ReservationConflictException(
                    $"The selected time conflicts with another reservation on table {reservation.TableNumber.Value}. A {ReservationGapMinutes}-minute buffer is required.");
            }
        }

        ValidateCreationStartTimeWindow(request.StartTime);
        ValidateCreationLeadTime(request.StartTime);
        ValidateDurationMinutes(request.ApproximateDurationMinutes);

        await reservationRepository.UpdateReservationTimeAsync(id, EnsureUtc(request.StartTime), request.ApproximateDurationMinutes);
    }

    public async Task UpdateReservationTableAsync(Guid id, UpdateReservationTableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservation = await reservationRepository.GetReservationByIdAsync(id);
        EnsureOpenReservation(reservation);

        if (!await reservationRepository.TableExistsAsync(reservation.RestaurantId, request.TableNumber))
        {
            throw new KeyNotFoundException($"Table {request.TableNumber} for restaurant {reservation.RestaurantId} not found");
        }

        var hasConflict = await reservationRepository.HasTableConflictAsync(
            reservation.RestaurantId,
            request.TableNumber,
            reservation.StartTime,
            reservation.ApproximateDurationMinutes,
            reservation.Id);

        if (hasConflict)
        {
            throw new ReservationConflictException(
                $"The selected table has a reservation conflict. A {ReservationGapMinutes}-minute buffer is required between reservations.");
        }

        await reservationRepository.UpdateReservationTableAsync(id, request.TableNumber);
    }

    public async Task UpdateReservationAssignedWaiterAsync(Guid id, UpdateReservationAssignedWaiterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.AssignedWaiterId);

        _ = await reservationRepository.GetReservationByIdAsync(id);
        await reservationRepository.UpdateReservationAssignedWaiterAsync(id, request.AssignedWaiterId.Trim());
    }

    public async Task UpdateReservationStatusAsync(Guid id, UpdateReservationStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservation = await reservationRepository.GetReservationByIdAsync(id);
        var newStatus = request.Status.Trim();

        if (!ReservationStatuses.Flow.Contains(newStatus, StringComparer.Ordinal))
            throw new ArgumentException($"Unsupported status '{newStatus}'.", nameof(request));

        var flow = ReservationStatuses.Flow;
        var currentIndex = flow.ToList().IndexOf(reservation.Status);
        var targetIndex = flow.ToList().IndexOf(newStatus);

        if (targetIndex != currentIndex + 1)
        {
            throw new InvalidReservationStatusTransitionException(
                $"Invalid status transition from '{reservation.Status}' to '{newStatus}'. Allowed flow: {string.Join(" -> ", ReservationStatuses.Flow)}.");
        }

        await reservationRepository.UpdateReservationStatusAsync(id, newStatus);
    }

    private static void EnsureGuestOwnership(Reservation reservation, string? currentUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentUserId);

        if (!string.Equals(reservation.GuestId, currentUserId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("You can only manage your own reservations.");
    }

    private static void EnsureOpenReservation(Reservation reservation)
    {
        if (!ReservationStatuses.OpenStatuses.Contains(reservation.Status))
            throw new InvalidOperationException("Reservation is closed and cannot be changed.");
    }

    private static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => dateTime.ToUniversalTime()
        };
    }

    private static void ValidateCreationStartTimeWindow(DateTime originalStartTime)
    {
        var startTime = TimeOnly.FromDateTime(originalStartTime);

        if (startTime < OpeningTime || startTime > ClosingTime)
        {
            throw new ArgumentException(
                "Reservation start time must be between 09:00 and 21:00 in the provided time zone context.",
                nameof(originalStartTime));
        }
    }

    private static void ValidateCreationLeadTime(DateTime startTime)
    {
        var utcStart = EnsureUtc(startTime);
        if (utcStart < DateTime.UtcNow.AddMinutes(15))
            throw new ArgumentException("Reservation start time must be at least 15 minutes in the future.", nameof(startTime));
    }

    private static void EnsureCancellationAllowed(Reservation reservation)
    {
        if (reservation.StartTime <= DateTime.UtcNow.AddMinutes(15))
            throw new InvalidOperationException("Reservation cannot be cancelled within 15 minutes of its start time.");
    }

    private static void ValidateDurationMinutes(int durationMinutes)
    {
        if (durationMinutes < MinReservationDurationMinutes || durationMinutes > MaxReservationDurationMinutes)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes), $"Reservation duration must be between {MinReservationDurationMinutes} and {MaxReservationDurationMinutes} minutes.");
    }
}


