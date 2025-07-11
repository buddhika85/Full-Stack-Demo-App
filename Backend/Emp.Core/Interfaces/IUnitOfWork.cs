using Emp.Core.Interfaces.Repositories;

namespace Emp.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IEmployeeRepository EmployeeRepository { get; }
    IDepartmentRepository DepartmentRepository { get; }
    Task<int> CompleteAsync();
}
