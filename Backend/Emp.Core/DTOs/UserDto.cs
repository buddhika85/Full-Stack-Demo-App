using Emp.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Emp.Core.DTOs;

public class UserDto : BaseDto
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



    public required UserRoles Role { get; set; }

    public bool IsActive { get; set; } = true;
}
