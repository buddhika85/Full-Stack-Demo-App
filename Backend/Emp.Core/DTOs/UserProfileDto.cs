using Emp.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Emp.Core.DTOs;

public class UserProfileDto : BaseDto
{
    [EmailAddress]
    public required string Username { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public UserRoles Role { get; set; }
}
