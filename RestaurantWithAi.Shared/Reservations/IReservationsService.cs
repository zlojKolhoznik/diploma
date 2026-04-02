namespace RestaurantWithAi.Shared.Reservations;

public interface IReservationsService
{
    Task<IEnumerable<ReservationResponse>> GetReservationsForGuestAsync(string guestId);
    Task<IEnumerable<ReservationResponse>> GetReservationsForWaiterAsync(string waiterId);
    Task<IEnumerable<ReservationResponse>> GetAllReservationsAsync();
    Task CreateReservationAsync(CreateReservationRequest request, string currentUserId, bool isAdminInitiated);
    Task DeleteReservationAsync(Guid id, string? currentUserId, bool isAdmin);
    Task UpdateReservationTimeAsync(Guid id, UpdateReservationTimeRequest request, string? currentUserId, bool isAdmin);
    Task UpdateReservationTableAsync(Guid id, UpdateReservationTableRequest request);
    Task UpdateReservationAssignedWaiterAsync(Guid id, UpdateReservationAssignedWaiterRequest request);
    Task UpdateReservationStatusAsync(Guid id, UpdateReservationStatusRequest request);
}

