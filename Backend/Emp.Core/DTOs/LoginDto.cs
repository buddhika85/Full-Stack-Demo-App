using System.ComponentModel.DataAnnotations;

namespace Emp.Core.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
