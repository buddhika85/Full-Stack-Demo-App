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

    public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
    {
        this.unitOfWork = unitOfWork;
        this.logger = logger;
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
        logger.LogInformation($"Attempting to get a user by Id {id}");
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
            logger.LogError(ex, "Error in getting all users");
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
                throw new InvalidOperationException($"Username '{userDto.Username}' already exists.");
            }

            var user = userDto.ToEntity();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
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


    public Task<string?> AuthenticateUserAsync(LoginDto loginDto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChangeUserPasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        throw new NotImplementedException();
    }


    public Task<bool> DeactivateUserAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<UserProfileDto?> GetUserProfileAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateUserAsync(int id, UpdateUserDto userDto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileDto userProfileDto)
    {
        throw new NotImplementedException();
    }
}
