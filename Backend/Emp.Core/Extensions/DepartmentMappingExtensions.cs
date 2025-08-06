using Emp.Core.DTOs;
using Emp.Core.Entities;

namespace Emp.Core.Extensions;

public static class DepartmentMappingExtensions
{

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


    public static IEnumerable<DepartmentEmpCountDto> ToEmpCountDtos(this IEnumerable<Department> departments)
    {
        return departments.Select(ToEmpCountDto);
    }

    public static DepartmentEmpCountDto ToEmpCountDto(this Department department)
    {
        return new DepartmentEmpCountDto
        {
            Department = department.Name,
            EmployeeCount = department.Employees.Count
        };
    }
}
