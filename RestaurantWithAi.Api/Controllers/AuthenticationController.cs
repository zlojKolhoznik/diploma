using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.Exceptions;

namespace RestaurantWithAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
	private readonly IAuthService _authService;

	public AuthenticationController(IAuthService authService)
	{
		_authService = authService;
	}

	[HttpPost("login")]
	[ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
	{
		try
		{
			var authResponse = await _authService.LoginAsync(request);
			return Ok(authResponse);
		}
		catch (AuthenticationFailedException ex)
		{
			return Unauthorized(new { message = ex.Message });
		}
		catch (Exception)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
		}
	}

	[HttpPost("register")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		try
		{
			await _authService.RegisterAsync(request);
			return NoContent();
		}
		catch (RegistrationFailedException ex)
		{
			return BadRequest(new { message = ex.Message });
		}
		catch (Exception)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
		}
	}
}
