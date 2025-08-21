using Emp.Core.DTOs;
using Emp.Core.Enums;
using Emp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Emp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{nameof(UserRoles.Admin)}")]        // all are admin only endpoints
public class UsersController : BaseController
{
    private readonly IUserService userService;
    private readonly ILogger<UsersController> logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        this.userService = userService;
        this.logger = logger;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        logger.LogInformation("API: GetUsers endpoint called by Admin.");
        try
        {
            var users = await userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Error in GetUsers endpoint.");
            return InternalServerError("Error in getting all users by admin");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        logger.LogInformation("API: GetUser endpoint called for ID: {Id} by Admin.", id);
        try
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user == null)
            {
                logger.LogWarning("API: User with ID {Id} not found.", id);
                return NotFoundError($"API: User with ID {id} not found.");
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Error in GetUser endpoint for ID: {Id}.", id);
            return InternalServerError("Error in getting a single user by admin");
        }
    }

    [HttpPost]

    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
    {
        logger.LogInformation("API: CreateUser endpoint called for username: {Username} by Admin.", createUserDto.Username);
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            logger.LogWarning("API: CreateUser validation failed. Errors: {Errors}", errors);
            return BadRequestError("Validation Errors", errors);
        }

        try
        {
            var createdUser = await userService.CreateUserAsync(createUserDto);
            if (createdUser == null)
            {
                logger.LogError("API: New user creation failed for username {Username}", createUserDto.Username);
                return InternalServerError($"New user creation failed for username {createUserDto.Username}");
            }
            logger.LogInformation("API: User '{Username}' created successfully with ID: {Id} by Admin.", createdUser.Username, createdUser.Id);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "API: Error in CreateUser endpoint for username: {Username}.", createUserDto.Username);
            return InternalServerError(ex.Message, $"API: Error in CreateUser endpoint for username: {createUserDto.Username}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Error in CreateUser endpoint for username: {Username}.", createUserDto.Username);
            return InternalServerError($"API: Error in CreateUser endpoint for username: {createUserDto.Username}.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
    {
        logger.LogInformation("API: UpdateUser endpoint called for ID: {Id} by Admin.", id);
        if (id != updateUserDto.Id)
        {
            logger.LogWarning("API: UpdateUser BadRequest - ID mismatch. Route ID: {RouteId}, DTO ID: {DtoId}.", id, updateUserDto.Id);
            return BadRequestError("ID",
                new List<string> {
                    $"ID mismatch.Route ID: {id}, DTO ID: {updateUserDto.Id}." });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            logger.LogWarning("API: UpdateUser validation failed for ID: {Id}. Errors: {Errors}", id, errors);
            return BadRequestError("Validation Errors", errors);
        }

        try
        {
            var success = await userService.UpdateUserAsync(id, updateUserDto);
            if (!success)
            {
                logger.LogWarning("API: Update failed: User with ID {Id} not found or no changes applied.", id);
                return InternalServerError($"API: Update failed: User with ID {id} not found or no changes applied.");
            }
            logger.LogInformation("API: User with ID {Id} updated successfully by Admin.", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Error in UpdateUser endpoint for ID: {Id}.", id);
            return InternalServerError(ex.Message, $"API: Error in UpdateUser endpoint for ID: {id}.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        logger.LogInformation("API: DeactivateUser endpoint called for ID: {Id} by Admin.", id);
        try
        {
            var success = await userService.DeactivateUserAsync(id);
            if (!success)
            {
                logger.LogWarning("API: Deactivate failed: User with ID {Id} not found.", id);
                return NotFoundError($"API: Deactivate failed: User with ID {id} not found.");
            }
            logger.LogInformation("API: User with ID {Id} deactivated successfully by Admin.", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Error in DeactivateUser endpoint for ID: {Id}.", id);
            return InternalServerError($"API: Error in DeactivateUser endpoint for ID: {id}.");
        }
    }
}
