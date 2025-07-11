using Emp.Core.Entities;

namespace Emp.Core.Interfaces.Repositories;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<IEnumerable<Employee>> GetAllEmployeesWithDepartmentAsync();
    Task<Employee> GetEmployeeWithDepartment(int id);
}
