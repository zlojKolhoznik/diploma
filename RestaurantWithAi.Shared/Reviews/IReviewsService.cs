using RestaurantWithAi.Shared.Reviews;

namespace RestaurantWithAi.Shared.Reviews;

public interface IReviewsService
{
    Task CreateReviewAsync(Guid reservationId, CreateReviewRequest request, string? currentUserId, bool isAdmin);
    Task<IEnumerable<ReviewResponse>> GetReviewsForRestaurantAsync(Guid restaurantId, GetReviewsQuery? query = null);
}

