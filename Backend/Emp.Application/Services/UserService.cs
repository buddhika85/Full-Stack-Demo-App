using Emp.Core;
using Emp.Core.DTOs;
using Emp.Core.Extensions;
using Emp.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Emp.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<UserService> logger;
    private readonly IJwtService jwtService;
    private readonly IPasswordHasherService passwordHasherService;

    public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IJwtService jwtService, IPasswordHasherService passwordHasherService)
    {
        this.unitOfWork = unitOfWork;
        this.logger = logger;
        this.jwtService = jwtService;
        this.passwordHasherService = passwordHasherService;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        logger.LogInformation("Attempting to get all users");
        try
        {
            var entities = await unitOfWork.UserRepository.GetAllAsync();
            logger.LogInformation("Successfully retrieved {Count} users.", entities.Count());
            return entities.ToDtos();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in getting all users");
            throw;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        logger.LogInformation("Attempting to get a user by id {id}", id);
        try
        {
            var entity = await unitOfWork.UserRepository.GetByIdAsync(id);
            if (entity == null)
            {
                logger.LogWarning("User with id {id} unavailable", id);
                return null;
            }
            logger.LogInformation("User with Id {id} retrieved.", id);
            return entity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in getting user by id {id}", id);
            throw;
        }
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserDto userDto)
    {
        logger.LogInformation("Attempting to create user with username/email: {Username}", userDto.Username);
        try
        {
            if (await unitOfWork.UserRepository.IsExistsAsync(userDto.Username))
            {
                logger.LogWarning("User creation failed: Username '{Username}' already exists.", userDto.Username);
                throw new InvalidOperationException($"Username '{userDto.Username}' already exists. Please use a unique username.");
            }

            var user = userDto.ToEntity();
            user.PasswordHash = passwordHasherService.HashPassword(userDto.Password);
            await unitOfWork.UserRepository.AddAsync(user);
            if (await unitOfWork.CompleteAsync() > 0)
            {
                logger.LogInformation("User with username/email {username} created successfully", userDto.Username);
                return user.ToDto();
            }
            logger.LogError("User with username/email {username} creation failed", userDto.Username);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating a user with username/email {username}", userDto.Username);
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto userDto)
    {
        logger.LogInformation("Attempting to update a user with id {UserId} and username/email {Username}", id, userDto.Username);
        try
        {
            var entity = await unitOfWork.UserRepository.GetByIdAsync(id);
            if (entity == null)
            {
                logger.LogError("User updated failed: User with id {UserId} unavailable", id);
                return false;
            }

            // Check if username is being changed to an existing one (excluding self)            
            if (!entity.Username.Equals(userDto.Username, StringComparison.OrdinalIgnoreCase))
            {
                var userByUsername = await unitOfWork.UserRepository.GetByUsernameAsync(userDto.Username);
                if (userByUsername != null && userByUsername.Id != id)
                {
                    logger.LogWarning("User update failed: Username '{Username}' already taken by another user.", userDto.Username);
                    throw new InvalidOperationException($"Username '{userDto.Username}' is already taken by another user.");
                }
            }

            userDto.MapToEntity(entity);
            unitOfWork.UserRepository.Update(entity);
            if (await unitOfWork.CompleteAsync() > 0)
            {
                logger.LogInformation("User with id {UserId} and username/email {Username} update successful", id, userDto.Username);
                return true;
            }

            logger.LogInformation("User with ID {UserId} username/email {Username} was found, but no changes were applied or saved.", id, entity.Username);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in updating User with id {UserId} and username/email {Username}", id, userDto.Username);
            throw;
        }
    }

    public async Task<string?> AuthenticateUserAsync(LoginDto loginDto)
    {
        logger.LogInformation("Authenticating user - {Username}", loginDto.Username);
        try
        {
            var user = await unitOfWork.UserRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                logger.LogError("User authetication failed: User with Username {Username} unavailable", loginDto.Username);
                return null;
            }

            if (!user.IsActive)
            {
                logger.LogError("User authetication failed: User with Username {Username} is not active", loginDto.Username);
                return null;
            }

            if (!passwordHasherService.VerifyPassword(loginDto.Password, user.PasswordHash))  // if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                logger.LogError("User authetication failed: Password does not match for user {Username}", loginDto.Username);
                return null;
            }

            logger.LogInformation("User authetication success: For user {Username}. Now generating JWT token.", loginDto.Username);
            var jwtToken = jwtService.GenerateJwtToken(user);
            logger.LogInformation("JWT token generation success: For user {Username}.", loginDto.Username);
            return jwtToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in autheticating User with id {Username}", loginDto.Username);
            throw;
        }
    }

    public async Task<bool> ChangeUserPasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        logger.LogInformation("Attempting to change password for user ID: {UserId}", userId);
        try
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("Change password failed: User with ID {UserId} not found.", userId);
                return false;
            }

            if (!passwordHasherService.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash)) //if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                logger.LogWarning("Change password failed for user ID {UserId}: Current password mismatch.", userId);
                return false;
            }

            user.PasswordHash = passwordHasherService.HashPassword(changePasswordDto.NewPassword);
            unitOfWork.UserRepository.Update(user);
            await unitOfWork.CompleteAsync();
            logger.LogInformation("Password for user ID {UserId} changed successfully.", userId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing password for user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ActivateDeactivateUserAsync(int id)
    {
        logger.LogInformation("Attempting to activate / deactivate user with ID: {UserId}", id);
        try
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                logger.LogWarning("Activate / Deactivate failed: User with ID {UserId} not found.", id);
                return false;
            }

            var logMsg = user.IsActive ? "deactivated" : "activated";

            user.IsActive = !user.IsActive;                     // toggleing user isActive status

            unitOfWork.UserRepository.Update(user);             // Mark for update
            await unitOfWork.CompleteAsync();
            logger.LogInformation("User with ID {UserId} {logMsg} successfully.", id, logMsg);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating user with ID: {UserId}", id);
            throw;
        }
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
    {
        logger.LogInformation("Attempting to retrieve user profile for ID: {UserId}", userId);
        try
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("User profile not found for ID: {UserId}", userId);
                return null;
            }
            logger.LogInformation("Successfully retrieved user profile for ID: {UserId}", userId);
            return user.ToProfileDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user profile for ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileDto userProfileDto)
    {
        logger.LogInformation("Attempting to update user profile for ID: {UserId}", userId);
        try
        {
            var existingUser = await unitOfWork.UserRepository.GetByIdAsync(userId);
            if (existingUser == null)
            {
                logger.LogWarning("User profile update failed: User with ID {UserId} not found.", userId);
                return false;
            }

            // Username (email) is NOT updatable via this profile endpoint.
            // The existingUser.Username is preserved.
            userProfileDto.MapToEntity(existingUser); // This mapping will only update FirstName and LastName
            unitOfWork.UserRepository.Update(existingUser);
            var affectedRows = await unitOfWork.CompleteAsync();

            if (affectedRows > 0)
            {
                logger.LogInformation("User profile for ID {UserId} updated successfully. Rows affected: {AffectedRows}", userId, affectedRows);
                return true;
            }
            else
            {
                logger.LogInformation("User profile for ID {UserId} was found, but no changes were applied or saved.", userId);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user profile for ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        logger.LogInformation($"Attempting to get a user by username {username}");
        try
        {
            var entity = await unitOfWork.UserRepository.GetByUsernameAsync(username);
            if (entity == null)
            {
                logger.LogWarning("User with username {username} unavailable", username);
                return null;
            }
            logger.LogInformation("User with username {username} retrieved.", username);
            return entity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in getting user by username {username}", username);
            throw;
        }
    }
}
