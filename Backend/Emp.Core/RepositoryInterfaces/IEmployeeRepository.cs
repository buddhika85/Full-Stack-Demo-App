using Emp.Core.Entities;

namespace Emp.Core.RepositoryInterfaces;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<IEnumerable<Employee>> GetAllEmployeesWithDepartmentAsync();
    Task<Employee> GetEmployeeWithDepartment(int id);
}
