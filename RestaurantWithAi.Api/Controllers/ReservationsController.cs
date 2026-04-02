using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController(IReservationsService reservationsService, ILogger<ReservationsController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Guest,Waiter")]
    [ProducesResponseType(typeof(IEnumerable<ReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ReservationResponse>>> GetReservations()
    {
        try
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User identity could not be determined." });

            IEnumerable<ReservationResponse> reservations;

            if (User.IsInRole("Waiter"))
                reservations = await reservationsService.GetReservationsForWaiterAsync(userId);
            else
                reservations = await reservationsService.GetReservationsForGuestAsync(userId);

            return Ok(reservations);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving reservations.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Guest,Admin")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReservationResponse>> CreateReservation([FromBody] CreateReservationRequest request)
    {
        try
        {
            string? guestId = null;
            if (User.IsInRole("Guest"))
            {
                guestId = User.Identity?.Name;
                if (string.IsNullOrEmpty(guestId))
                    return Unauthorized(new { message = "User identity could not be determined." });
            }

            var result = await reservationsService.CreateReservationAsync(request, guestId);
            return CreatedAtAction(nameof(GetReservations), result);
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Create reservation request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while creating a reservation.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Guest,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelReservation(Guid id)
    {
        try
        {
            var guestId = User.IsInRole("Guest") ? User.Identity?.Name : null;
            var isAdmin = User.IsInRole("Admin");

            await reservationsService.CancelReservationAsync(id, guestId, isAdmin);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogInformation(ex, "Unauthorized access to reservation {ReservationId}.", id);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Cannot cancel reservation {ReservationId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while cancelling reservation {ReservationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{id:guid}/time")]
    [Authorize(Roles = "Guest,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateReservationTime(Guid id, [FromBody] UpdateReservationTimeRequest request)
    {
        try
        {
            var guestId = User.IsInRole("Guest") ? User.Identity?.Name : null;
            var isAdmin = User.IsInRole("Admin");

            await reservationsService.UpdateReservationTimeAsync(id, request, guestId, isAdmin);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogInformation(ex, "Unauthorized access to reservation {ReservationId}.", id);
            return Forbid();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("overlaps"))
        {
            logger.LogInformation(ex, "Time update for reservation {ReservationId} causes overlap.", id);
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Cannot update time for reservation {ReservationId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating time for reservation {ReservationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{id:guid}/table")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("overlapping"))
        {
            logger.LogInformation(ex, "Table update for reservation {ReservationId} causes overlap.", id);
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Cannot update table for reservation {ReservationId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating table for reservation {ReservationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{id:guid}/assigned-waiter")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateReservationWaiter(Guid id, [FromBody] UpdateReservationWaiterRequest request)
    {
        try
        {
            await reservationsService.UpdateReservationWaiterAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Cannot update waiter for reservation {ReservationId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating waiter for reservation {ReservationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateReservationStatus(Guid id, [FromBody] UpdateReservationStatusRequest request)
    {
        try
        {
            await reservationsService.UpdateReservationStatusAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Invalid status transition for reservation {ReservationId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating status for reservation {ReservationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }
}
