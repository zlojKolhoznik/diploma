using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IReservationRepository
{
    Task<IEnumerable<Reservation>> GetReservationsByGuestIdAsync(string guestId);
    Task<IEnumerable<Reservation>> GetReservationsByWaiterIdAsync(string waiterId);
    Task<Reservation> GetReservationByIdAsync(Guid id);
    Task<Reservation> AddReservationAsync(Reservation reservation);
    Task<bool> HasOverlappingReservationAsync(Guid restaurantId, int tableNumber, DateTimeOffset startTime, int durationMinutes, Guid? excludeReservationId = null);
    Task<IEnumerable<int>> GetAvailableTableNumbersAsync(Guid restaurantId, DateTimeOffset startTime, int durationMinutes);
    Task<bool> HasAvailableTablesAsync(Guid restaurantId, DateTimeOffset startTime, int durationMinutes);
    Task UpdateReservationAsync(Reservation reservation);
}
