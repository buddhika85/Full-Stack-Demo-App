using Emp.Core.DTOs;

namespace Emp.Core.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto userDto);
    Task<bool> UpdateUserAsync(int id, UpdateUserDto userDto);
    Task<bool> DeactivateUserAsync(int id); // Changed from Delete to Deactivate
    Task<string?> AuthenticateUserAsync(LoginDto loginDto); // For JWT token generation
    Task<UserProfileDto?> GetUserProfileAsync(int userId); // For "View Profile"
    Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileDto userProfileDto);
    Task<bool> ChangeUserPasswordAsync(int userId, ChangePasswordDto changePasswordDto);
}
