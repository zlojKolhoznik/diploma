namespace RestaurantWithAi.Shared.Reservations;

public interface IReservationsService
{
    Task<IEnumerable<ReservationResponse>> GetReservationsByGuestAsync(string guestUserId);
    Task<IEnumerable<ReservationResponse>> GetReservationsByWaiterAsync(string waiterId);
    Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request, string? guestUserId);
    Task DeleteReservationAsync(Guid id);
    Task UpdateReservationTimeAsync(Guid id, UpdateReservationTimeRequest request);
    Task UpdateReservationTableAsync(Guid id, UpdateReservationTableRequest request);
    Task UpdateReservationWaiterAsync(Guid id, UpdateReservationWaiterRequest request);
    Task UpdateReservationStatusAsync(Guid id, UpdateReservationStatusRequest request);
}
