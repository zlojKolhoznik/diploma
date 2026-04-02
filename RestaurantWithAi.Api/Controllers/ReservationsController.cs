using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController(IReservationsService reservationsService, ILogger<ReservationsController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Guest,Waiter,Admin")]
    [ProducesResponseType(typeof(IEnumerable<ReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ReservationResponse>>> GetReservations()
    {
        try
        {
            var userId = GetCurrentUserId();

            if (User.IsInRole("Admin"))
                return Ok(await reservationsService.GetAllReservationsAsync());

            if (User.IsInRole("Waiter"))
                return Ok(await reservationsService.GetReservationsForWaiterAsync(userId));

            return Ok(await reservationsService.GetReservationsForGuestAsync(userId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving reservations.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Guest,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
    {
        try
        {
            await reservationsService.CreateReservationAsync(request, GetCurrentUserId(), User.IsInRole("Admin"));
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Create reservation request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogInformation(ex, "Create reservation request is not permitted for current user.");
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Create reservation failed because related entity was not found.");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while creating reservation.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Guest,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteReservation(Guid id)
    {
        try
        {
            await reservationsService.DeleteReservationAsync(id, GetCurrentUserId(), User.IsInRole("Admin"));
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Delete reservation failed for id {ReservationId} because reservation is closed.", id);
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation with id {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while deleting reservation {ReservationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{id:guid}/time")]
    [Authorize(Roles = "Guest,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateReservationTime(Guid id, [FromBody] UpdateReservationTimeRequest request)
    {
        try
        {
            await reservationsService.UpdateReservationTimeAsync(id, request, GetCurrentUserId(), User.IsInRole("Admin"));
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ReservationConflictException ex)
        {
            logger.LogInformation(ex, "Update reservation time failed due to overlap for reservation {ReservationId}.", id);
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Update reservation time failed for id {ReservationId} because reservation is closed.", id);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Update reservation time request for id {ReservationId} was invalid.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation with id {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating reservation time {ReservationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{id:guid}/table")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateReservationTable(Guid id, [FromBody] UpdateReservationTableRequest request)
    {
        try
        {
            await reservationsService.UpdateReservationTableAsync(id, request);
            return NoContent();
        }
        catch (ReservationConflictException ex)
        {
            logger.LogInformation(ex, "Update reservation table failed due to overlap for reservation {ReservationId}.", id);
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Update reservation table failed for id {ReservationId} because reservation is closed.", id);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Update reservation table request for id {ReservationId} was invalid.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation or table was not found for reservation {ReservationId}.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating reservation table {ReservationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{id:guid}/assigned-waiter")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAssignedWaiter(Guid id, [FromBody] UpdateReservationAssignedWaiterRequest request)
    {
        try
        {
            await reservationsService.UpdateReservationAssignedWaiterAsync(id, request);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Update assigned waiter request for reservation {ReservationId} was invalid.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation with id {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating assigned waiter for reservation {ReservationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateReservationStatusRequest request)
    {
        try
        {
            await reservationsService.UpdateReservationStatusAsync(id, request);
            return NoContent();
        }
        catch (InvalidReservationStatusTransitionException ex)
        {
            logger.LogInformation(ex, "Invalid reservation status transition for reservation {ReservationId}.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Update reservation status request for id {ReservationId} was invalid.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation with id {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating reservation status {ReservationId}.", id);
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

