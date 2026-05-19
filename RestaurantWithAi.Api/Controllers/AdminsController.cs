using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Shared.Admins;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admins")]
public class AdminsController(IAdminService adminService, ILogger<AdminsController> logger) : ControllerBase
{
    [HttpPost("appoint")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AppointAdmin([FromBody] AppointAdminRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await adminService.AppointAdminAsync(currentUserId, request.AdminUserIdToAppoint, request.RestaurantId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Appoint admin request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Appoint admin failed due to business logic violation.");
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogInformation(ex, "Appoint admin failed due to authorization constraint.");
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Appoint admin failed because a user was not found.");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while appointing admin.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpDelete("demote/{adminToDemoteId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DemoteAdmin(string adminToDemoteId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await adminService.DemoteAdminAsync(currentUserId, adminToDemoteId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Demote admin request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogInformation(ex, "Demote admin failed due to hierarchy constraint.");
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Demote admin failed because relationship was not found.");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while demoting admin.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("appointed")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetAppointedAdmins()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var appointedAdmins = await adminService.GetAppointedAdminsAsync(currentUserId);
            return Ok(appointedAdmins);
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Get appointed admins request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving appointed admins.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("appointed-by")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string?>> GetAppointedBy()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var appointedBy = await adminService.GetAppointedByAsync(currentUserId);
            if (appointedBy == null)
                return NoContent();
            return Ok(appointedBy);
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Get appointed-by request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving appointed-by information.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedAdminUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedAdminUsersResponse>> GetUsers(
        [FromQuery] UserGroup role = UserGroup.Customer,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var users = await adminService.GetUsersByRoleAsync(role, page, pageSize);
            return Ok(users);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            logger.LogInformation(ex, "Get users request has invalid paging parameters.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving users.");
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

