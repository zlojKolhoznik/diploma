namespace RestaurantWithAi.Shared.Reservations;

public interface IReservationsService
{
    Task<IEnumerable<ReservationResponse>> GetReservationsForGuestAsync(string guestId);
    Task<IEnumerable<ReservationResponse>> GetReservationsForWaiterAsync(string waiterId);
    Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request, string? guestId);
    Task CancelReservationAsync(Guid id, string? guestId, bool isAdmin);
    Task UpdateReservationTimeAsync(Guid id, UpdateReservationTimeRequest request, string? guestId, bool isAdmin);
    Task UpdateReservationTableAsync(Guid id, UpdateReservationTableRequest request);
    Task UpdateReservationWaiterAsync(Guid id, UpdateReservationWaiterRequest request);
    Task UpdateReservationStatusAsync(Guid id, UpdateReservationStatusRequest request);
}
