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
    public async Task<ActionResult<IEnumerable<RestaurantBrief>>> GetRestaurants(
        [FromQuery] string? city,
        [FromQuery] DateTime? time,
        [FromQuery(Name = "duration")] int? durationMinutes)
    {
        try
        {
            var restaurants = await restaurantsService.GetRestaurantsAsync(city, time, durationMinutes);
            return Ok(restaurants);
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Invalid restaurants query parameters.");
            return BadRequest(new { message = ex.Message });
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RestaurantDetail>> GetRestaurant(
        Guid id,
        [FromQuery] DateTime? time,
        [FromQuery(Name = "duration")] int? durationMinutes)
    {
        try
        {
            var restaurant = await restaurantsService.GetRestaurantDetailAsync(id, time, durationMinutes);
            return Ok(restaurant);
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Invalid restaurant detail query parameters for id {RestaurantId}.", id);
            return BadRequest(new { message = ex.Message });
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

    [HttpPost("{id:guid}/image")]
    [HttpPatch("{id:guid}/image")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadRestaurantImage(Guid id, [FromForm] IFormFile? file)
    {
        try
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { message = "No file provided." });

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Invalid file type. Only images are allowed." });

            const long maxSizeBytes = 10 * 1024 * 1024; // 10 MB
            if (file.Length > maxSizeBytes)
                return BadRequest(new { message = "File is too large. Maximum allowed size is 10MB." });

            await using var stream = file.OpenReadStream();
            var url = await restaurantsService.UploadRestaurantImageAsync(id, stream, file.FileName, file.ContentType);
            return Ok(new { url });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Restaurant image upload failed because restaurant was not found.");
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogInformation(ex, "Unauthorized while uploading restaurant image.");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while uploading restaurant image.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }
}

