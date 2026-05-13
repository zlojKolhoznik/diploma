using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Reviews;

namespace RestaurantWithAi.Core.Services;

public class ReviewService(IReviewRepository reviewRepository, IReservationRepository reservationRepository, IRestaurantRepository restaurantRepository, IWaiterRepository waiterRepository, IReviewModerationService reviewModerationService, IMapper mapper) : IReviewsService
{
    public async Task CreateReviewAsync(Guid reservationId, CreateReviewRequest request, string currentUserId, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Ensure reservation exists
        var reservation = await reservationRepository.GetReservationByIdAsync(reservationId);
        if (reservation is null)
            throw new KeyNotFoundException("Reservation not found.");

        // Prevent duplicate review per reservation
        if (await reviewRepository.GetByReservationIdAsync(reservationId) is not null)
            throw new InvalidOperationException("A review for this reservation already exists.");

        var moderationResult = await reviewModerationService.ModerateAsync(request);
        if (!moderationResult.Approved)
        {
            var message = moderationResult.Reason ?? "The review did not pass moderation.";
            if (!string.IsNullOrWhiteSpace(moderationResult.SuggestedRephrasing))
                message = $"{message} Suggested rephrasing: {moderationResult.SuggestedRephrasing}";

            throw new InvalidOperationException(message);
        }

        var review = mapper.Map<Review>(request);
        review.ReservationId = reservationId;
        review.CreatedAtUtc = DateTime.UtcNow;

        await reviewRepository.AddReviewAsync(review);

        // Update restaurant average rating
        var restaurantAverage = await reviewRepository.GetAverageRestaurantRatingAsync(reservation.RestaurantId);
        var restaurant = await restaurantRepository.GetRestaurantByIdAsync(reservation.RestaurantId);
        restaurant.AverageRating = restaurantAverage;
        await restaurantRepository.SaveChangesAsync();

        // Update waiter average rating if a waiter is assigned
        if (!string.IsNullOrWhiteSpace(reservation.AssignedWaiterId))
        {
            var waiterAverage = await reviewRepository.GetAverageWaiterRatingAsync(reservation.AssignedWaiterId);
            var waiter = await waiterRepository.GetWaiterByUserIdAsync(reservation.AssignedWaiterId);
            waiter.AverageRating = waiterAverage;
            await waiterRepository.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ReviewResponse>> GetReviewsForRestaurantAsync(Guid restaurantId)
    {
        var reviews = await reviewRepository.GetReviewsForRestaurantAsync(restaurantId);
        return mapper.Map<IEnumerable<ReviewResponse>>(reviews);
    }
}


