using System.ComponentModel.DataAnnotations;

namespace Emp.Core.Entities;

public class BaseEntity
{
    [Key]
    public int Id { get; set; }
}
