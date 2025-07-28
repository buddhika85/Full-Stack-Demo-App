using Emp.Core.DTOs;
using Emp.Core.Entities;
using Emp.Core.Enums;
using System.Data;

namespace Emp.Core.Extensions;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = Enum.Parse<UserRoles>(user.Role), // Convert string to enum
            IsActive = user.IsActive
        };
    }

    public static IEnumerable<UserDto> ToDtos(this IEnumerable<User> users)
    {
        return users.Select(ToDto);
    }

    public static User ToEntity(this CreateUserDto createUserDto)
    {
        return new User
        {
            Username = createUserDto.Username,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Role = createUserDto.Role.ToString(), // Convert enum to string for storage
            IsActive = true // New users are active by default
        };
    }

    public static void MapToEntity(this UpdateUserDto updateUserDto, User user)
    {
        if (updateUserDto == null || user == null)
            return;

        user.Username = updateUserDto.Username;
        user.FirstName = updateUserDto.FirstName;
        user.LastName = updateUserDto.LastName;
        user.Role = updateUserDto.Role.ToString(); // Convert enum to string for storage
        user.IsActive = updateUserDto.IsActive;
    }

    public static UserProfileDto? ToProfileDto(this User user)
    {
        if (user == null)
            return null;
        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = Enum.Parse<UserRoles>(user.Role) // Convert string to enum
        };
    }

    public static void MapToEntity(this UpdateUserProfileDto userProfileDto, User user)
    {
        if (userProfileDto == null || user == null) return;
        // Username (email) is intentionally not mapped here, we will not allow changing username/email as per requirements
        user.FirstName = userProfileDto.FirstName;
        user.LastName = userProfileDto.LastName;
    }
}
