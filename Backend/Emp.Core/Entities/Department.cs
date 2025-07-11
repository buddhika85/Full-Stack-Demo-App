using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Core.Entities;


public class BaseEntity
{
    [Key]
    public int Id { get; set; }
}

public class Department : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    public virtual ICollection<Employee> Employees { get; set; } = [];
}

public class Employee : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public required string EmailAddress { get; set; }

    [Required]
    [ForeignKey("DepartmentId")]
    public int DepartmentId { get; set; }

    public virtual required Department Department { get; set; }

}
