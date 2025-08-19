using Emp.Core.DTOs;
using Emp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace Emp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IUserService userService;
    private readonly ILogger<AuthController> logger;
    private readonly IJwtService jwtService;

    public AuthController(IUserService userService, ILogger<AuthController> logger, IJwtService jwtService)
    {
        this.userService = userService;
        this.logger = logger;
        this.jwtService = jwtService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        logger.LogInformation("API: Login attempt for user: {Username}", loginDto.Username);
        try
        {
            var token = await userService.AuthenticateUserAsync(loginDto);

            if (string.IsNullOrWhiteSpace(token))
            {
                logger.LogWarning("API: Login failed for user: {Username}. Invalid credentials.", loginDto.Username);
                return UnauthorizedError("Invalid username or password.");
            }

            // Retrieve user details to return in the response            
            var user = await userService.GetUserByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                logger.LogError("API: Authenticated user '{Username}' not found in database after token generation. This is an unexpected state.", loginDto.Username);
                return InternalServerError("Internal server error during login process.");
            }

            logger.LogInformation("API: User '{Username}' logged in successfully.", loginDto.Username);
            return Ok(new LoginResponseDto { Token = token, User = user });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Error during login for user: {Username}", loginDto.Username);
            return InternalServerError("Internal server error during login.");
        }
    }


    /// <summary>
    /// Logs out the current user. The implementation depends on the authentication scheme.
    /// </summary>
    /// <returns>An Ok result confirming the user has been logged out.</returns>
    [Authorize] // This ensures only authenticated users can call this endpoint.
    [HttpPost("logout")]
    public async Task<ActionResult<LogoutResponseDto>> Logout()
    {
        // Get the user ID from the claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            logger.LogWarning("API: GetUserProfile: User ID claim not found or invalid for authenticated user.");
            return UnauthorizedError("User ID not found in token.");
        }

        logger.LogInformation("API: Logout endpoint called for User ID: {UserId}", userId);

        // --- SCENARIO 1: Session-based Authentication ---
        // If you are using session-based authentication (e.g., cookies), this is the
        // standard way to sign the user out. It clears the session cookie from the browser
        // and revokes the server-side session.
        //
        // await HttpContext.SignOutAsync();
        // return Ok(new { Message = "Logged out successfully (session-based)." });


        // --- SCENARIO 2: Token-based Authentication (JWT) with Blacklisting ---
        // For JWTs, simply having the client delete the token is often sufficient.
        // However, to immediately invalidate a token on the server side (e.g., when a
        // user changes their password), you can use a revocation list.
        //
        // We get the token from the request headers.
        try
        {
            var userProfile = await userService.GetUserProfileAsync(userId);
            if (userProfile == null)
            {
                logger.LogWarning("API: Logout unsuccessful - User profile not found for User ID: {UserId}", userId);
                return NotFoundError("User profile not found.");
            }
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (!string.IsNullOrEmpty(accessToken))
            {
                // Add the token to our blacklist. This makes it impossible for 
                // this specific token to be used for any future requests.
                // NOTE: This approach requires checking the blacklist on every future request.
                jwtService.BlackListToken(accessToken);
            }

            return Ok(new LogoutResponseDto { Username = userProfile.Username, LoggedOut = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Logout unsuccessful - Error retrieving user for User ID: {UserId}", userId);
            return InternalServerError("Logout unsuccessful - Internal server error retrieving user.");
        }

    }

    [Authorize] // Only authenticated users can access their own profile
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile()
    {
        // Get the user ID from the claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            logger.LogWarning("API: GetUserProfile: User ID claim not found or invalid for authenticated user.");
            return UnauthorizedError("User ID not found in token.");
        }

        logger.LogInformation("API: GetUserProfile endpoint called for User ID: {UserId}", userId);
        try
        {
            var userProfile = await userService.GetUserProfileAsync(userId);
            if (userProfile == null)
            {
                logger.LogWarning("API: User profile not found for User ID: {UserId}", userId);
                return NotFoundError("User profile not found.");
            }
            return Ok(userProfile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Error retrieving user profile for User ID: {UserId}", userId);
            return InternalServerError("Internal server error retrieving user profile.");
        }
    }


    [Authorize] // Only authenticated users can update their own profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto userProfileDto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            logger.LogWarning("API: UpdateUserProfile: User ID claim not found or invalid for authenticated user.");
            return UnauthorizedError("User ID not found in token.");
        }

        logger.LogInformation("API: UpdateUserProfile endpoint called for User ID: {UserId}", userId);
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            logger.LogWarning("API: UpdateUserProfile validation failed for User ID: {UserId}. Errors: {Errors}", userId, errors);
            return BadRequestError("Validation Issues", errors);
        }

        try
        {
            var success = await userService.UpdateUserProfileAsync(userId, userProfileDto);
            if (!success)
            {
                logger.LogWarning("API: UpdateUserProfile failed for User ID: {UserId}. Service returned false.", userId);
                return InternalServerError("Failed to update user profile or no changes were applied.");
            }
            logger.LogInformation("API: User profile for User ID: {UserId} updated successfully.", userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Error updating user profile for User ID: {UserId}", userId);
            return InternalServerError("Internal server error updating user profile.");
        }
    }



    [Authorize] // Only authenticated users can change their password
    [HttpPut("profile/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            logger.LogWarning("API: ChangePassword: User ID claim not found or invalid for authenticated user.");
            return UnauthorizedError("User ID not found in token.");
        }

        logger.LogInformation("API: ChangePassword endpoint called for User ID: {UserId}", userId);
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            logger.LogWarning("API: ChangePassword validation failed for User ID: {UserId}. Errors: {Errors}", userId, errors);
            return BadRequestError("Validation Issues", errors);
        }

        try
        {
            var success = await userService.ChangeUserPasswordAsync(userId, changePasswordDto);
            if (!success)
            {
                logger.LogWarning("API: ChangePassword failed for User ID: {UserId}. Current password mismatch or other issue.", userId);
                return InternalServerError("Failed to change password. Current password might be incorrect.");
            }
            logger.LogInformation("API: Password for User ID: {UserId} changed successfully.", userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Error changing password for User ID: {UserId}", userId);
            return InternalServerError("Internal server error changing password.");
        }
    }
}
