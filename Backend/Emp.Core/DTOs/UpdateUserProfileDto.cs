using System.ComponentModel.DataAnnotations;

namespace Emp.Core.DTOs;

public class UpdateUserProfileDto
{
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }
}
