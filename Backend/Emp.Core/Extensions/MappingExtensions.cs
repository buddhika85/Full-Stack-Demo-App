using Emp.Core.DTOs;
using Emp.Core.Entities;

namespace Emp.Core.Extensions;

public static class MappingExtensions
{
    #region employee
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
    #endregion

    #region department
    public static DepartmentDto ToDto(this Department department)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name
        };
    }

    public static IEnumerable<DepartmentDto> ToDtos(this IEnumerable<Department> departments)
    {
        return departments.Select(ToDto);
    }

    public static Department ToEntity(this CreateDepartmentDto createDepartmentDto)
    {
        return new Department
        {
            Name = createDepartmentDto.Name
        };
    }

    // For updating an existing Department entity from an UpdateDepartmentDto
    public static void MapToEntity(this UpdateDepartmentDto updateDepartmentDto, Department department)
    {
        if (updateDepartmentDto == null || department == null) return;

        // Note: Id is not mapped here as it's for an existing entity
        department.Name = updateDepartmentDto.Name;
    }
    #endregion 
}
