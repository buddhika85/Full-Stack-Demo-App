using Emp.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Emp.Core.DTOs;

public class UpdateUserDto : BaseDto
{
    [Required]
    [MaxLength(50)]
    [EmailAddress]
    public required string Username { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }

    [Required]
    public required UserRoles Role { get; set; }
    public bool IsActive { get; set; }
}
