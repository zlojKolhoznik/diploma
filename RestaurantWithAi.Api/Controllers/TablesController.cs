using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Tables;

namespace RestaurantWithAi.Api.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId:guid}/tables")]
[Authorize(Roles = "Admin")]
public class TablesController(ITablesService tablesService, ILogger<TablesController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TableBrief>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TableBrief>>> GetTables(Guid restaurantId)
    {
        try
        {
            var tables = await tablesService.GetTablesAsync(restaurantId);
            return Ok(tables);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Restaurant with id {RestaurantId} was not found.", restaurantId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving tables for restaurant {RestaurantId}.", restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<TableBrief>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TableBrief>>> GetAvailableTables(
        Guid restaurantId,
        [FromQuery] DateTime? time,
        [FromQuery(Name = "duration")] int? durationMinutes)
    {
        if (!time.HasValue)
            return BadRequest(new { message = "Query parameter 'time' is required." });

        if (!durationMinutes.HasValue)
            return BadRequest(new { message = "Query parameter 'duration' is required." });

        try
        {
            var tables = await tablesService.GetAvailableTablesAsync(restaurantId, time.Value, durationMinutes.Value);
            return Ok(tables);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Restaurant with id {RestaurantId} was not found.", restaurantId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving available tables for restaurant {RestaurantId}.", restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddTable(Guid restaurantId, [FromBody] AddTableRequest request)
    {
        try
        {
            await tablesService.AddTableAsync(restaurantId, request);
            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Add table request for restaurant {RestaurantId} was invalid.", restaurantId);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Restaurant with id {RestaurantId} was not found.", restaurantId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while adding a table to restaurant {RestaurantId}.", restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpDelete("{tableNumber:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTable(Guid restaurantId, int tableNumber)
    {
        try
        {
            await tablesService.DeleteTableAsync(restaurantId, tableNumber);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Table {TableNumber} for restaurant {RestaurantId} was not found.", tableNumber, restaurantId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while deleting table {TableNumber} from restaurant {RestaurantId}.", tableNumber, restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("{tableNumber:int}/seats")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTableSeats(Guid restaurantId, int tableNumber, [FromBody] UpdateTableSeatsRequest request)
    {
        try
        {
            await tablesService.UpdateTableSeatsAsync(restaurantId, tableNumber, request);
            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            logger.LogInformation(ex, "Update table seats request for table {TableNumber} in restaurant {RestaurantId} was invalid.", tableNumber, restaurantId);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Table {TableNumber} for restaurant {RestaurantId} was not found.", tableNumber, restaurantId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating seats for table {TableNumber} in restaurant {RestaurantId}.", tableNumber, restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }
}
