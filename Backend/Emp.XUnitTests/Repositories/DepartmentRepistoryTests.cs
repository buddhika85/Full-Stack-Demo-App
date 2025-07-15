using Emp.Infrastructure.Data;
using Emp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Emp.XUnitTests.TestData;
using Emp.Core.Entities;

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
        var testDbContext = GetInMemoryDbContext("GetAllAsync_ReturnsAllDepartments_WhenCalled");
        var repository = new DepartmentRepository(testDbContext);

        // act        
        var departmentsActual = await repository.GetAllAsync();

        // assert
        departmentsActual.Should().NotBeNull();
        departmentsActual.Should().HaveCount(4);                                    // DbContext seeds 4 departments
        departmentsActual.Should().Contain(x => x.Name == "Human Resources");       // as per seeding
        departmentsActual.Should().Contain(x => x.Name == "Engineering");           // as per seeding
    }

    [Theory]
    [InlineData(1, "Human Resources")]
    [InlineData(2, "Engineering")]
    public async Task GetByIdAsync_ReturnsNotNull_IfExists(int departmentId, string expectedName)
    {
        // arrange
        var testDbContext = GetInMemoryDbContext("GetByIdAsync_ReturnsNotNull_IfExists");
        var repository = new DepartmentRepository(testDbContext);

        // act
        var actual = await repository.GetByIdAsync(departmentId);


        // assert
        actual.Should().NotBeNull();
        actual.Id.Should().Be(departmentId);
        actual.Name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(10001)]
    public async Task GetByIdAsync_ReturnsNull_IfNonExistent(int departmentId)
    {
        // arrange
        var testDbContext = GetInMemoryDbContext("GetByIdAsync_ReturnsNull_IfNonExistent");
        var repository = new DepartmentRepository(testDbContext);

        // act
        var actual = await repository.GetByIdAsync(departmentId);


        // assert
        actual.Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(DepartmentTestData))]
    public async Task GetByIdAsync_ReturnsCorrectDeparment_ForIdPassed(Department expected)
    {
        // arrange
        var testDbContext = GetInMemoryDbContext("GetByIdAsync_ReturnsNull_IfNonExistent");
        var repository = new DepartmentRepository(testDbContext);

        // act
        var actual = await repository.GetByIdAsync(expected.Id);

        // assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task AddAsync_AddsNewDepartmentToDB_WhenCalled()
    {
        // arrange
        var testDbContext = GetInMemoryDbContext("AddAsync_AddsNewRecordToDB_WhenCalled");
        var repository = new DepartmentRepository(testDbContext);
        var testDepartment = new Department { Name = "Test Department" };

        // act
        await repository.AddAsync(testDepartment);
        await testDbContext.SaveChangesAsync();

        // assert        
        testDbContext.Departments.Should().HaveCount(4 + 1);                      // seeded 4, with newly added 1 it should be 5
        var newDepartment = await repository.GetByIdAsync(testDepartment.Id);
        newDepartment.Should().NotBeNull();
        newDepartment.Id.Should().Be(testDepartment.Id);
        newDepartment.Name.Should().Be(testDepartment.Name);
    }

    [Fact]
    public async Task Update_UpdatesDepartment_IfExists()
    {
        // arrange
        var testDbContext = GetInMemoryDbContext("Update_UpdatesDepartment_IfExists");
        var repository = new DepartmentRepository(testDbContext);
        var departmentToUpdate = await repository.GetByIdAsync(1);

        departmentToUpdate.Should().NotBeNull();

        // act
        departmentToUpdate.Name = $"{departmentToUpdate.Name} updated";
        repository.Update(departmentToUpdate);
        await testDbContext.SaveChangesAsync();

        // assert
        var departmentUpdated = await repository.GetByIdAsync(departmentToUpdate.Id);
        departmentUpdated.Should().NotBeNull();
        departmentUpdated.Id.Should().Be(departmentToUpdate.Id);
        departmentUpdated.Name.Should().Be(departmentToUpdate.Name);
    }
}
