using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IReviewRepository
{
    Task<Review?> GetByReservationIdAsync(Guid reservationId);
    Task<IEnumerable<Review>> GetReviewsForRestaurantAsync(Guid restaurantId);
    Task AddReviewAsync(Review review);
    Task SaveChangesAsync();
    Task<decimal?> GetAverageRestaurantRatingAsync(Guid restaurantId);
    Task<decimal?> GetAverageWaiterRatingAsync(string waiterId);
}

