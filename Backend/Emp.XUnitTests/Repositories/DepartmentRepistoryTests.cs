using Emp.Infrastructure.Data;
using Emp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Emp.XUnitTests.Repositories;

public class DepartmentRepistoryTests
{
    // Helper method to create a new DbContextOptions for an in-memory database
    // Each test should use a unique database name to ensure isolation
    private ApplicationDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        var context = new ApplicationDbContext(options);
        // Ensure the database is clean for each test, though unique names help
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated(); // Creates the schema and seeds data from OnModelCreating
        return context;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllDepartments_WhenCalled()
    {
        // arrange
        var testDbContext = GetInMemoryDbContext("GetAllDepartmentsTest");
        var repository = new DepartmentRepository(testDbContext);

        // act
        var departmentExpected = await testDbContext.Departments.ToListAsync();
        var departmentsActual = await repository.GetAllAsync();

        // assert
        departmentsActual.Should().NotBeNull();
        departmentsActual.Should().HaveSameCount(departmentExpected);
        departmentsActual.Should().BeEquivalentTo(departmentExpected);
    }
}
