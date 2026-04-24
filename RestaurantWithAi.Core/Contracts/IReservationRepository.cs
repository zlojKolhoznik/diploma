using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IReservationRepository
{
    Task<IEnumerable<Reservation>> GetReservationsForGuestAsync(string guestId);
    Task<IEnumerable<Reservation>> GetReservationsForWaiterAsync(string waiterId);
    Task<IEnumerable<Reservation>> GetAllReservationsAsync();
    Task<Reservation> GetReservationByIdAsync(Guid id);
    Task AddReservationAsync(Reservation reservation);
    Task DeleteReservationAsync(Guid id);
    Task UpdateReservationTimeAsync(Guid id, DateTime startTime, int durationMinutes);
    Task UpdateReservationTableAsync(Guid id, int? tableNumber);
    Task UpdateReservationAssignedWaiterAsync(Guid id, string? waiterId);
    Task UpdateReservationStatusAsync(Guid id, string status);
    Task<bool> RestaurantExistsAsync(Guid restaurantId);
    Task<bool> TableExistsAsync(Guid restaurantId, int tableNumber);
    Task<bool> HasTableConflictAsync(Guid restaurantId, int tableNumber, DateTime startTime, int durationMinutes, Guid? excludedReservationId = null);
    Task<IEnumerable<Table>> GetAvailableTablesAsync(Guid restaurantId, DateTime startTime, int durationMinutes, int? minimumSeats = null);
    Task<bool> HasAvailableTablesAsync(Guid restaurantId, DateTime startTime, int durationMinutes, int? minimumSeats = null);
    Task<Guid?> GetWaiterRestaurantIdAsync(string waiterId);
}

