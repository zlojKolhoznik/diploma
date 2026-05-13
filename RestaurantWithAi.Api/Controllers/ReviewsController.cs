using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Reviews;

namespace RestaurantWithAi.Api.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId:guid}/reviews")]
public class ReviewsController(IReviewsService reviewsService, IReviewModerationService reviewModerationService, ILogger<ReviewsController> logger) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetReviews(Guid restaurantId)
    {
        try
        {
            var reviews = await reviewsService.GetReviewsForRestaurantAsync(restaurantId);
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving reviews for restaurant {RestaurantId}.", restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("../../reservations/{reservationId:guid}/review")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateReview(Guid restaurantId, Guid reservationId, [FromBody] CreateReviewRequest request)
    {
        try
        {
            await reviewsService.CreateReviewAsync(reservationId, request, GetCurrentUserId(), User.IsInRole("Admin"));
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Create review request for reservation {ReservationId} in restaurant {RestaurantId} was invalid.", reservationId, restaurantId);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Create review failed for reservation {ReservationId} in restaurant {RestaurantId} because a related entity was not found.", reservationId, restaurantId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Create review failed for reservation {ReservationId} in restaurant {RestaurantId} due to state conflict.", reservationId, restaurantId);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while creating review for reservation {ReservationId} in restaurant {RestaurantId}.", reservationId, restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("../../reservations/{reservationId:guid}/review/moderate")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewModerationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReviewModerationResult>> ModerateReview(Guid restaurantId, Guid reservationId, [FromBody] CreateReviewRequest request)
    {
        try
        {
            var result = await reviewModerationService.ModerateAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Moderating review draft for reservation {ReservationId} in restaurant {RestaurantId} was invalid.", reservationId, restaurantId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while moderating review draft for reservation {ReservationId} in restaurant {RestaurantId}.", reservationId, restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Current user identifier claim is missing.");

        return userId;
    }
}

