using Emp.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Emp.Core.DTOs;

public class CreateUserDto
{
    [Required]
    [MaxLength(50)]
    [EmailAddress]
    public required string Username { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }

    [Required]
    public UserRoles Role { get; set; } = UserRoles.Staff;
}
