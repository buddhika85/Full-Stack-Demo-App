using System.ComponentModel.DataAnnotations;


namespace Emp.Core.Entities;

public class User : BaseEntity
{
    [Required]
    [MaxLength(50)]
    [EmailAddress]
    public required string Username { get; set; }


    // When we later verify a password using BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash),
    // the BCrypt.Net library intelligently extracts the salt from the storedPasswordHash string
    // and uses it to re-hash the enteredPassword for comparison.
    // This is one of the convenient features of BCrypt. We do not need to manually store the salt in DB. More secured.
    [Required]
    public string PasswordHash { get; set; } = string.Empty;            // Uses Bcrypt.Hashing - so salt and hash and combined and stored


    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }


    [Required]
    [MaxLength(20)]
    public required string Role { get; set; }

    public bool IsActive { get; set; } = true;
}
