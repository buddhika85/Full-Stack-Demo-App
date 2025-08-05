using Emp.Core.DTOs;
using Emp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.Api.Controllers;


public class AuthController : BaseController
{
    private readonly IUserService userService;
    private readonly ILogger<AuthController> logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        this.userService = userService;
        this.logger = logger;
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
}
