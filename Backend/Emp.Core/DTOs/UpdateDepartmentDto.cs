using Emp.Core.ValidationAttributes;
using System.ComponentModel.DataAnnotations;


namespace Emp.Core.DTOs;

public class UpdateDepartmentDto : BaseDto
{
    [Required]
    [MaxLength(100)]
    [FirstLetterUpperCase(ErrorMessage = "Department name must start with an uppercase letter.")] // Applied custom validation
    public string Name { get; set; } = string.Empty;
}
