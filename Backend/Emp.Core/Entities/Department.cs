using System.ComponentModel.DataAnnotations;

namespace Emp.Core.Entities;

public class Department : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    public virtual ICollection<Employee> Employees { get; set; } = [];
}
