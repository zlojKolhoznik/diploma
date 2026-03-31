using Amazon.CognitoIdentityProvider.Model;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.Exceptions;

namespace RestaurantWithAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(IAuthService authService, ILogger<AuthenticationController> logger) : ControllerBase
{
	[HttpPost("login")]
	[ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
	{
		try
		{
			var authResponse = await authService.LoginAsync(request);
			logger.LogInformation("Login request succeeded");
			return Ok(authResponse);
		}
		catch (AuthenticationFailedException ex)
		{
			logger.LogInformation("Authentication failed");
			return Unauthorized(new { message = ex.Message });
		}
		catch (NotAuthorizedException ex)
		{
			logger.LogInformation("Not authorized");
			return Unauthorized(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unexpected error occurred.");
			return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
		}
	}

	[HttpPost("register")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		try
		{
			await authService.RegisterAsync(request);
			logger.LogInformation("Registration request succeeded");
			return NoContent();
		}
		catch (RegistrationFailedException ex)
		{
			logger.LogInformation("Registration failed");
			return BadRequest(new { message = ex.Message });
		}
		catch (InvalidPasswordException ex)
		{
			logger.LogInformation("Invalid password");
			return BadRequest(new { message = ex.Message });
		}
		catch (DuplicateEmailException ex)
		{
			logger.LogInformation("Duplicate email registration attempt");
			return Conflict(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unexpected error occurred.");
			return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
		}
	}
}
