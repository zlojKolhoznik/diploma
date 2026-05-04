using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IReviewRepository
{
    Task<Review?> GetByReservationIdAsync(Guid reservationId);
    Task<IEnumerable<Review>> GetReviewsForRestaurantAsync(Guid restaurantId);
    Task AddReviewAsync(Review review);
    Task SaveChangesAsync();
}

