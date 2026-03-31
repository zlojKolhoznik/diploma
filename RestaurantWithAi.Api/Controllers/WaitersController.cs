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
    [ProducesResponseType(typeof(IEnumerable<WaiterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<WaiterDto>>> GetWaiters()
    {
        try
        {
            var waiters = await waiterService.GetAllWaitersAsync();
            logger.LogInformation("Retrieved all waiters successfully");
            return Ok(waiters);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving waiters.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("{userId}")]
    [ProducesResponseType(typeof(WaiterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WaiterDto>> AssignWaiterRole(string userId, [FromBody] AssignWaiterRequest request)
    {
        try
        {
            var waiter = await waiterService.AssignWaiterRoleAsync(userId, request.RestaurantId);
            logger.LogInformation("Waiter role assigned to user {UserId}", SanitizeForLog(userId));
            return CreatedAtAction(nameof(GetWaiters), waiter);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while assigning waiter role to user {UserId}.", SanitizeForLog(userId));
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("restaurant/{restaurantId:int}")]
    [ProducesResponseType(typeof(WaiterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WaiterDto>> AssignWaiterToRestaurant(int restaurantId, [FromBody] AssignToRestaurantRequest request)
    {
        try
        {
            var waiter = await waiterService.AssignWaiterToRestaurantAsync(request.UserId, restaurantId);
            logger.LogInformation("Waiter {UserId} assigned to restaurant {RestaurantId}", SanitizeForLog(request.UserId), restaurantId);
            return Ok(waiter);
        }
        catch (WaiterNotFoundException ex)
        {
            logger.LogInformation("Waiter not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while assigning waiter to restaurant {RestaurantId}.", restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    private static string SanitizeForLog(string value) =>
        value.Replace("\r", string.Empty, StringComparison.Ordinal)
             .Replace("\n", string.Empty, StringComparison.Ordinal);
}
