using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Auth;

namespace RestaurantWithAi.Api.Controllers;

using Microsoft.AspNetCore.Http;

[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController(IAuthService authService, IProfileService profileService, ILogger<ProfileController> logger) : ControllerBase
{
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            await authService.UpdateProfileAsync(userId, request);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Update profile request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogInformation(ex, "Update profile failed because user was not found.");
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Update profile failed due to service error.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating user profile.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile? file)
    {
        try
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { message = "No file provided." });

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Invalid file type. Only images are allowed." });

            const long maxSizeBytes = 5 * 1024 * 1024; // 5 MB
            if (file.Length > maxSizeBytes)
                return BadRequest(new { message = "File is too large. Maximum allowed size is 5MB." });

            var userId = GetCurrentUserId();
            await using var stream = file.OpenReadStream();
            var result = await profileService.UploadProfileImageAsync(userId, stream, file.FileName, file.ContentType, TimeSpan.FromMinutes(15));

            return Ok(new { url = result.PresignedUrl });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogInformation(ex, "Unauthorized while uploading profile image.");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while uploading profile image.");
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

