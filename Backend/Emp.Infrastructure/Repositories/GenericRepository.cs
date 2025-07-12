using Emp.Core.Entities;
using Emp.Core.Interfaces.Repositories;
using Emp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Emp.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext context;
    private readonly DbSet<T> dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        this.context = context;
        dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await dbSet.AsNoTracking().ToListAsync();        // not for editing - as no tracking
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await dbSet.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        dbSet.Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        context.Remove(entity);
    }

    public async Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
    {
        return await dbSet.Where(predicate).ToListAsync();
    }
}
