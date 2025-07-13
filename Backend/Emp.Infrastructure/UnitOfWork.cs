using Emp.Core;
using Emp.Core.Interfaces.Repositories;
using Emp.Infrastructure.Data;
namespace Emp.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext context;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentsRepository;

    public IEmployeeRepository EmployeeRepository => _employeeRepository;
    public IDepartmentRepository DepartmentRepository => _departmentsRepository;

    public UnitOfWork(ApplicationDbContext context, IEmployeeRepository employeeRepository, IDepartmentRepository departmentRepository)
    {
        this.context = context;
        _employeeRepository = employeeRepository;
        _departmentsRepository = departmentRepository;
    }

    public async Task<int> CompleteAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();                      // releasing memory manualy (GC cannot clean DB connections)
        GC.SuppressFinalize(this);              // Since context of this class is disposed, it pointless GC running its Finerlizer (Destructor) again
    }
}
