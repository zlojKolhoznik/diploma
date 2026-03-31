using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class WaitersController(IWaiterService waiterService, ILogger<WaitersController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WaiterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<WaiterResponse>>> GetAllWaiters()
    {
        try
        {
            var waiters = await waiterService.GetAllWaitersAsync();
            logger.LogInformation("Get all waiters request succeeded");
            return Ok(waiters);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignWaiterRole(string userId, [FromBody] AssignWaiterRoleRequest request)
    {
        try
        {
            await waiterService.AssignWaiterRoleAsync(userId);

            if (!string.IsNullOrEmpty(request.RestaurantId))
            {
                await waiterService.AssignWaiterToRestaurantAsync(request.RestaurantId, userId);
            }

            logger.LogInformation("Assign waiter role request succeeded");
            return NoContent();
        }
        catch (UserNotFoundException ex)
        {
            logger.LogInformation("User not found during waiter role assignment");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{userId}/restaurant")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignWaiterToRestaurant(
        string userId,
        [FromBody] AssignWaiterToRestaurantRequest request)
    {
        try
        {
            await waiterService.AssignWaiterToRestaurantAsync(request.RestaurantId, userId);
            logger.LogInformation("Assign waiter to restaurant request succeeded");
            return NoContent();
        }
        catch (UserNotFoundException ex)
        {
            logger.LogInformation("User not found during waiter-to-restaurant assignment");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }
}
