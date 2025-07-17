using Emp.Core.Entities;

namespace Emp.Core.Interfaces.Repositories;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    public Task<IEnumerable<Department>> GetAllDepartmentsWithEmployeesAsync();
    public Task<Department?> GetDepartmentWithEmployeesAsync(int id);
}
