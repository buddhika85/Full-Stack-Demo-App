using Emp.Core.Entities;
using Emp.Core.Interfaces.Repositories;
using Emp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Emp.Infrastructure.Repositories;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsWithEmployeesAsync()
    {
        return await dbSet.Include(x => x.Employees).ToListAsync();
    }

    public async Task<Department?> GetDepartmentWithEmployeesAsync(int id)
    {
        return await dbSet.Include(x => x.Employees).SingleOrDefaultAsync(x => x.Id == id);
    }
}
