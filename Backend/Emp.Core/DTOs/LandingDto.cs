namespace Emp.Core.DTOs;

public class LandingDto
{
    public IReadOnlyList<DepartmentEmpCountDto> Departments { get; set; } = new List<DepartmentEmpCountDto>();
    public int DepartmentsCount => Departments.Count();
    public int EmployeeCount => Departments.Sum(x => x.EmployeeCount);
}

public class DepartmentEmpCountDto
{
    public required string Department { get; set; }
    public int EmployeeCount { get; set; }
}
