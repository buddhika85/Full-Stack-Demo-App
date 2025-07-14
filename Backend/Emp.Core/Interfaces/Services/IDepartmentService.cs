using Emp.Core.DTOs;

namespace Emp.Core.Interfaces.Services;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
    Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
    Task<DepartmentDto?> CreateDepartmentAsync(CreateDepartmentDto departmentDto);
    Task<bool> UpdateDepartmentAsync(int id, UpdateDepartmentDto departmentDto);
    Task<bool> DeleteDepartmentAsync(int id);
}
