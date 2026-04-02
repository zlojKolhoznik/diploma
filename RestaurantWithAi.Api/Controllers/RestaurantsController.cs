using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Restaurants;

namespace RestaurantWithAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController(IRestaurantsService restaurantsService, ILogger<RestaurantsController> logger) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<RestaurantBrief>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RestaurantBrief>>> GetRestaurants([FromQuery] string? city, [FromQuery] DateTime? time, [FromQuery] int? duration)
    {
        if ((time == null) != (duration == null))
        {
            var missing = time == null ? "'time'" : "'duration'";
            return BadRequest(new { message = $"{missing} query parameter is required when the other is specified." });
        }

        try
        {
            var restaurants = await restaurantsService.GetRestaurantsAsync(city, time, duration);
            return Ok(restaurants);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving restaurants.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RestaurantDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RestaurantDetail>> GetRestaurant(Guid id, [FromQuery] DateTime? time, [FromQuery] int? duration)
    {
        if ((time == null) != (duration == null))
        {
            var missing = time == null ? "'time'" : "'duration'";
            return BadRequest(new { message = $"{missing} query parameter is required when the other is specified." });
        }

        try
        {
            var restaurant = await restaurantsService.GetRestaurantDetailAsync(id, time, duration);
            return Ok(restaurant);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Restaurant with id {RestaurantId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving restaurant {RestaurantId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantRequest request)
    {
        try
        {
            await restaurantsService.CreateRestaurantAsync(request);
            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Create restaurant request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while creating a restaurant.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRestaurant(Guid id, [FromBody] CreateRestaurantRequest request)
    {
        try
        {
            await restaurantsService.UpdateRestaurantAsync(id, request);
            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Update restaurant request for id {RestaurantId} was invalid.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Restaurant with id {RestaurantId} was not found for update.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating restaurant {RestaurantId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRestaurant(Guid id)
    {
        try
        {
            await restaurantsService.DeleteRestaurantAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Restaurant with id {RestaurantId} was not found for deletion.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while deleting restaurant {RestaurantId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }
}

