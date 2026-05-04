using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Reviews;

namespace RestaurantWithAi.Core.Services;

public class ReviewService(IReviewRepository reviewRepository, IReservationRepository reservationRepository, IMapper mapper) : IReviewsService
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

        var review = mapper.Map<Review>(request);
        review.ReservationId = reservationId;
        review.CreatedAtUtc = DateTime.UtcNow;

        await reviewRepository.AddReviewAsync(review);
    }

    public async Task<IEnumerable<ReviewResponse>> GetReviewsForRestaurantAsync(Guid restaurantId)
    {
        var reviews = await reviewRepository.GetReviewsForRestaurantAsync(restaurantId);
        return mapper.Map<IEnumerable<ReviewResponse>>(reviews);
    }
}


