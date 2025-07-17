using Emp.Core.Entities;
using Emp.Core.Interfaces.Repositories;
using Emp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Emp.Infrastructure.Repositories;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesWithDepartmentAsync()
    {
        return await dbSet.Include(x => x.Department).ToListAsync();
    }

    public async Task<Employee?> GetEmployeeWithDepartmentAsync(int id)
    {
        return await dbSet.Include(x => x.Department).SingleOrDefaultAsync(x => x.Id == id);
    }
}
