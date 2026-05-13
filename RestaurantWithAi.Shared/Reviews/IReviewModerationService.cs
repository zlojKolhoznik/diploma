namespace RestaurantWithAi.Shared.Reviews;

public interface IReviewModerationService
{
    Task<ReviewModerationResult> ModerateAsync(CreateReviewRequest request, CancellationToken cancellationToken = default);
}

