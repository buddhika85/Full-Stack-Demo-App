using Emp.Core.Entities;

namespace Emp.Core.Interfaces.Repositories;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    public Task<IEnumerable<Department>> GetAllDepartmentsWithEmployees();
    public Task<Department?> GetDepartmentWithEmployees(int id);
}
