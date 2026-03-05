using FinancePlanner.Application.DTOs;
using FinancePlanner.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancePlanner.API.Controllers;

[Route("api/[controller]")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Called after MSAL sign-in to sync the Entra user into your database.
    /// Creates the user record if it doesn't exist yet.
    /// </summary>
    [HttpPost("sync")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncUser()
    {
        var objectId = GetCurrentUserObjectId(); // Entra object ID string
        var email = User.FindFirst("preferred_username")?.Value
                 ?? User.FindFirst("email")?.Value;
        var firstName = User.FindFirst("given_name")?.Value;
        var lastName = User.FindFirst("family_name")?.Value;

        var user = await _authService.SyncEntraUserAsync(objectId, email, firstName, lastName);
        return Ok(user);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        _logger.LogInformation("User logged out: {UserId}", GetCurrentUserObjectId());
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _authService.GetCurrentUserAsync(GetCurrentUserId());
        return user == null ? NotFound() : Ok(user);
    }
}