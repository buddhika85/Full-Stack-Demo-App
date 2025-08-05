using Emp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emp.Infrastructure.Data;

// Go to API project
// dotnet ef migrations add InitialCreate --project  ..\Emp.Infrastructure --startup-project .
// dotnet ef database update --project ..\Emp.Infrastructure --startup-project .


// revert to 20250714040404_InitialCreate migration
// dotnet ef database update 20250714040404_InitialCreate --project  ..\Emp.Infrastructure --startup-project .
// remove all unapplied migrations
// dotnet ef migrations remove --project  ..\Emp.Infrastructure --startup-project .

// dotnet ef migrations add UserAuth  --project  ..\Emp.Infrastructure --startup-project .
// dotnet ef database update --project ..\Emp.Infrastructure --startup-project .
public class ApplicationDbContext : DbContext
{
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<User> Users { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure unique index for Username (email) in User table
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // during runtime IDs values will be auto generated, but not during seeding
        modelBuilder.Entity<User>()
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<Department>()
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<Employee>()
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();


        // Foriegn Key Relationships
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);  // deletion of department will throw an error, if it still has atleast one employee
    }
}
