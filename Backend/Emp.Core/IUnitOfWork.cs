using Emp.Core.Interfaces.Repositories;

namespace Emp.Core;

public interface IUnitOfWork : IDisposable
{
    public IEmployeeRepository EmployeeRepository { get; }

    public IDepartmentRepository DepartmentRepository { get; }

    public IUserRepository UserRepository { get; }

    Task<int> CompleteAsync();
}
