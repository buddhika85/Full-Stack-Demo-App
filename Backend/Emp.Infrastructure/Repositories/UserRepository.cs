using Emp.Core.Entities;
using Emp.Core.Interfaces.Repositories;
using Emp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Emp.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await dbSet.SingleOrDefaultAsync(x => x.Username == username);
    }
}
