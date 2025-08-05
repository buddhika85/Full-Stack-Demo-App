using Emp.Core.Entities;
using Emp.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Emp.Infrastructure.Data;

public static class ApplicationDbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!await context.Users.AnyAsync())
        {
            await SeedUsers(context);
        }

        if (!await context.Departments.AnyAsync())
        {
            await SeedDepartments(context);
        }

        if (!await context.Employees.AnyAsync())
        {
            await SeedEmployees(context);
        }
    }

    private static async Task SeedUsers(ApplicationDbContext context)
    {
        var admin = new User
        {
            Username = "admin@emp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRoles.Admin.ToString(),
            IsActive = true,
            FirstName = "Admin",
            LastName = "User"
        };

        var staff = new User
        {
            Username = "staff@emp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
            Role = UserRoles.Staff.ToString(),
            IsActive = true,
            FirstName = "Staff",
            LastName = "Member"
        };

        context.Users.AddRange(admin, staff);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDepartments(ApplicationDbContext context)
    {
        var departments = new List<Department>
        {
            new Department { Name = "Human Resources" },
            new Department { Name = "Engineering" },
            new Department { Name = "Marketing" },
            new Department { Name = "Sales" }
        };
        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();
    }

    private static async Task SeedEmployees(ApplicationDbContext context)
    {
        var employees = new List<Employee>
        {
            new Employee { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", DepartmentId = 2 },
            new Employee { FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", DepartmentId = 1 },
            new Employee { FirstName = "Peter", LastName = "Jones", Email = "peter.jones@example.com", DepartmentId = 2 }
        };
        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();
    }
}
