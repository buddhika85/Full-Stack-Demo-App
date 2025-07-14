using Emp.Core.Entities;
using Microsoft.EntityFrameworkCore;


namespace Emp.Infrastructure.Data;

// dotnet ef migrations add InitialCreate --project  ..\Emp.Infrastructure --startup-project .
// dotnet ef database update --project ..\Emp.Infrastructure --startup-project .
public class ApplicationDbContext : DbContext
{
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);  // deletion of department will throw an error, if it still has atleast one employee

        // Seed initial data for Departments
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Human Resources" },
            new Department { Id = 2, Name = "Engineering" },
            new Department { Id = 3, Name = "Marketing" },
            new Department { Id = 4, Name = "Sales" }
        );

        // Seed initial data for Employees
        modelBuilder.Entity<Employee>().HasData(
            new Employee { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", DepartmentId = 2 },
            new Employee { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", DepartmentId = 1 },
            new Employee { Id = 3, FirstName = "Peter", LastName = "Jones", Email = "peter.jones@example.com", DepartmentId = 2 }
        );
    }
}
