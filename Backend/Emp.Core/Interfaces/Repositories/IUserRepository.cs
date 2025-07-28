using Emp.Core.Entities;

namespace Emp.Core.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    public Task<User?> GetByUsernameAsync(string username);

    public Task<bool> IsExistsAsync(string username);
}
