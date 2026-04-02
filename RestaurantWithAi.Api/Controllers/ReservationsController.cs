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
    [Authorize(Roles = "Customer,Waiter")]
    [ProducesResponseType(typeof(IEnumerable<ReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ReservationResponse>>> GetReservations()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? string.Empty;

            if (User.IsInRole("Waiter"))
            {
                var waiterReservations = await reservationsService.GetReservationsByWaiterAsync(userId);
                return Ok(waiterReservations);
            }

            var guestReservations = await reservationsService.GetReservationsByGuestAsync(userId);
            return Ok(guestReservations);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving reservations.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Customer,Admin")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReservationResponse>> CreateReservation([FromBody] CreateReservationRequest request)
    {
        try
        {
            if (User.IsInRole("Customer") && string.IsNullOrWhiteSpace(request.GuestName))
            {
                var userId = User.FindFirst("sub")?.Value;
                var created = await reservationsService.CreateReservationAsync(request, userId);
                return CreatedAtAction(nameof(GetReservations), created);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.GuestName))
                    return BadRequest(new { message = "GuestName is required when Admin creates a reservation." });

                var created = await reservationsService.CreateReservationAsync(request, null);
                return CreatedAtAction(nameof(GetReservations), created);
            }
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Create reservation request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Resource not found while creating reservation.");
            return NotFound(new { message = ex.Message });
        }
        catch (ReservationConflictException ex)
        {
            logger.LogInformation(ex, "Reservation conflict detected.");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while creating a reservation.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Customer,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteReservation(Guid id)
    {
        try
        {
            await reservationsService.DeleteReservationAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation with id {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Reservation with id {ReservationId} cannot be cancelled.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while deleting reservation {ReservationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{id:guid}/time")]
    [Authorize(Roles = "Customer,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateReservationTime(Guid id, [FromBody] UpdateReservationTimeRequest request)
    {
        try
        {
            await reservationsService.UpdateReservationTimeAsync(id, request);
            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Update reservation time request for id {ReservationId} was invalid.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation with id {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (ReservationConflictException ex)
        {
            logger.LogInformation(ex, "Reservation conflict detected for id {ReservationId}.", id);
            return Conflict(new { message = ex.Message });
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
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Update reservation table request for id {ReservationId} was invalid.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Resource not found for reservation {ReservationId}.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (ReservationConflictException ex)
        {
            logger.LogInformation(ex, "Reservation conflict detected for id {ReservationId}.", id);
            return Conflict(new { message = ex.Message });
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateReservationWaiter(Guid id, [FromBody] UpdateReservationWaiterRequest request)
    {
        try
        {
            await reservationsService.UpdateReservationWaiterAsync(id, request);
            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Update reservation waiter request for id {ReservationId} was invalid.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Resource not found for reservation {ReservationId}.", id);
            return NotFound(new { message = ex.Message });
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateReservationStatus(Guid id, [FromBody] UpdateReservationStatusRequest request)
    {
        try
        {
            await reservationsService.UpdateReservationStatusAsync(id, request);
            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Update reservation status request for id {ReservationId} was invalid.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Reservation with id {ReservationId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidStatusTransitionException ex)
        {
            logger.LogInformation(ex, "Invalid status transition for reservation {ReservationId}.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating status for reservation {ReservationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }
}
