using System.ComponentModel.DataAnnotations;


namespace Emp.Core.Entities;

public class User : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public required string Username { get; set; }

    [Required]
    public required string PasswordHash { get; set; }

    [Required]
    [MaxLength(20)]
    public required string Role { get; set; }

    public bool IsActive { get; set; } = true;
}
