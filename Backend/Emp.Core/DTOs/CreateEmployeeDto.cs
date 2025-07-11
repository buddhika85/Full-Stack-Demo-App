using Emp.Core.ValidationAttributes;
using System.ComponentModel.DataAnnotations;


namespace Emp.Core.DTOs;

public class CreateEmployeeDto
{
    [Required]
    [MaxLength(100)]
    [FirstLetterUpperCase(ErrorMessage = "First name must start with an uppercase letter.")] // Applied custom validation
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public int DepartmentId { get; set; }
}
