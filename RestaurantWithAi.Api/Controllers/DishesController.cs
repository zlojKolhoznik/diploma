using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Dishes;

namespace RestaurantWithAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DishesController(IDishesService dishesService, ILogger<DishesController> logger) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DishBrief>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DishBrief>>> GetDishes()
    {
        try
        {
            var dishes = await dishesService.GetDishesAsync();
            return Ok(dishes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving dishes.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DishDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DishDetail>> GetDish(Guid id)
    {
        try
        {
            var dish = await dishesService.GetDishDetailAsync(id);
            return Ok(dish);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Dish with id {DishId} was not found.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving dish {DishId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateDish([FromBody] CreateDishRequest request)
    {
        try
        {
            await dishesService.CreateDishAsync(request);
            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Create dish request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while creating a dish.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateDish(Guid id, [FromBody] CreateDishRequest request)
    {
        try
        {
            await dishesService.UpdateDishAsync(id, request);
            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Update dish request for id {DishId} was invalid.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Dish with id {DishId} was not found for update.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating dish {DishId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{id:guid}/availability")]
    [Authorize(Roles = "Waiter,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateDishAvailability(Guid id, [FromBody] IEnumerable<Guid> restaurantIds)
    {
        try
        {
            await dishesService.UpdateDishAvailabilityAsync(id, restaurantIds);
            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Update dish availability request for id {DishId} was invalid.", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Dish availability update failed for dish id {DishId}.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating dish availability for dish {DishId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteDish(Guid id)
    {
        try
        {
            await dishesService.DeleteDishAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Dish with id {DishId} was not found for deletion.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while deleting dish {DishId}.", id);
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
    public async Task<IActionResult> UploadDishImage(Guid id, [FromForm] IFormFile? file)
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
            var url = await dishesService.UploadDishImageAsync(id, stream, file.FileName, file.ContentType);
            return Ok(new { url });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Dish image upload failed because dish was not found.");
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogInformation(ex, "Unauthorized while uploading dish image.");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while uploading dish image.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }
}

