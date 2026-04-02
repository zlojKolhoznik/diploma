using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Core.Contracts;

public interface IReservationRepository
{
    Task<IEnumerable<Reservation>> GetReservationsByGuestAsync(string guestUserId);
    Task<IEnumerable<Reservation>> GetReservationsByWaiterAsync(string waiterId);
    Task<Reservation> GetReservationByIdAsync(Guid id);
    Task<Reservation> CreateReservationAsync(Reservation reservation);
    Task DeleteReservationAsync(Guid id);
    Task UpdateReservationTimeAsync(Guid id, DateTime startTime, int durationMinutes);
    Task UpdateReservationTableAsync(Guid id, int tableNumber);
    Task UpdateReservationWaiterAsync(Guid id, string? waiterId);
    Task UpdateReservationStatusAsync(Guid id, ReservationStatus status);
    Task<bool> HasOverlapAsync(Guid restaurantId, int tableNumber, DateTime startTime, int durationMinutes, Guid? excludeReservationId = null);
}
