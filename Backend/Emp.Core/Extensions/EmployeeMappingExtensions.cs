using Emp.Core.DTOs;
using Emp.Core.Entities;


namespace Emp.Core.Extensions;

public static class EmployeeMappingExtensions
{

    // For updating an existing Employee entity from an UpdateEmployeeDto
    public static void MapToEntity(this UpdateEmployeeDto updateEmployeeDto, Employee employee)
    {
        if (updateEmployeeDto == null || employee == null) return;

        // Note: Id is not mapped here as it's for an existing entity
        employee.FirstName = updateEmployeeDto.FirstName;
        employee.LastName = updateEmployeeDto.LastName;
        employee.Email = updateEmployeeDto.Email;
        employee.DepartmentId = updateEmployeeDto.DepartmentId;
    }
    public static EmployeeDto ToDto(this Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee?.Department?.Name ?? string.Empty
        };
    }

    public static IEnumerable<EmployeeDto> ToDtos(this IEnumerable<Employee> employees)
    {
        return employees.Select(ToDto);     // same as x => ToDto(x)
    }

    public static Employee ToEntity(this CreateEmployeeDto createEployeeDto)
    {
        return new Employee
        {
            FirstName = createEployeeDto.FirstName,
            LastName = createEployeeDto.LastName,
            Email = createEployeeDto.Email,
            DepartmentId = createEployeeDto.DepartmentId
        };
    }
}
