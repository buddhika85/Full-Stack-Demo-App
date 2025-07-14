using Emp.Core.DTOs;

namespace Emp.Core.Interfaces.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeDto employeeDto);
    Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto employeeDto);
    Task<bool> DeleteEmployeeAsync(int id);
}
