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
    private async Task<ApplicationDbContext> GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        var context = new ApplicationDbContext(options);


        // Ensure the database is clean for each test, though unique names help
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated(); // Creates the schema and seeds data from OnModelCreating

        await ApplicationDbSeeder.SeedAsync(context); // Seed initial data here              

        return context;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllDepartments_WhenCalled()
    {
        // arrange
        var testDbContext = await GetInMemoryDbContext("GetAllAsync_ReturnsAllDepartments_WhenCalled");
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
        var testDbContext = await GetInMemoryDbContext("GetByIdAsync_ReturnsNotNull_IfExists");
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
        var testDbContext = await GetInMemoryDbContext("GetByIdAsync_ReturnsNull_IfNonExistent");
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
        var testDbContext = await GetInMemoryDbContext("GetByIdAsync_ReturnsNull_IfNonExistent");
        var repository = new DepartmentRepository(testDbContext);

        // act
        var actual = await repository.GetByIdAsync(expected.Id);

        // assert
        actual.Should().NotBeNull();
        actual.Id.Should().Be(expected.Id);
        actual.Name.Should().BeEquivalentTo(expected.Name);
    }

    [Fact]
    public async Task AddAsync_AddsNewDepartmentToDB_WhenCalled()
    {
        // arrange
        var testDbContext = await GetInMemoryDbContext("AddAsync_AddsNewRecordToDB_WhenCalled");
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
        var testDbContext = await GetInMemoryDbContext("Update_UpdatesDepartment_IfExists");
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

    [Theory]
    [InlineData(3)]         // department Id 3 and 4 does not have any employees, so should be deletable
    [InlineData(4)]
    public async Task Delete_DeletesDepartment_IfExists(int departmentId)
    {
        // arrange
        var testDbContext = await GetInMemoryDbContext("Delete_DeletesDepartment_IfExists");
        var repository = new DepartmentRepository(testDbContext);
        var departmentToDelete = await repository.GetByIdAsync(departmentId);
        var departmentCount = (await repository.GetAllAsync()).Count();

        departmentToDelete.Should().NotBeNull();

        // act
        repository.Delete(departmentToDelete);
        await testDbContext.SaveChangesAsync();

        // assert
        var departmentCountAfterDelete = (await repository.GetAllAsync()).Count();
        var afterDelete = await repository.GetByIdAsync(departmentId);
        afterDelete.Should().BeNull();
        departmentCountAfterDelete.Should().Be(departmentCount - 1);
    }

    [Fact]
    public async Task FindAsync_ReturnsFilteredDepartments_IfExistsForPredicate()
    {
        // arrange
        var testDbContext = await GetInMemoryDbContext("FindAsync_ReturnsFilteredDepartments_IfExistsForPredicate");
        var repository = new DepartmentRepository(testDbContext);

        // act
        var filteredDepartments = await repository.FindAsync(x => x.Name.Equals("human Resources", StringComparison.OrdinalIgnoreCase));

        // assert
        filteredDepartments.Should().NotBeNull();
        filteredDepartments.Should().ContainSingle();             // as per seeded data there is only 1 Human Resources
        filteredDepartments.ToList()[0].Name.ToLower().Should().BeEquivalentTo("human resources");
    }

    [Fact]
    public async Task FindAsync_ReturnsZeroDepartments_IfNonExistsForPredicate()
    {
        // arrange
        var testDbContext = await GetInMemoryDbContext("FindAsync_ReturnsZeroDepartments_IfNonExistsForPredicate");
        var repository = new DepartmentRepository(testDbContext);

        // act
        var filteredDepartments = await repository.FindAsync(x => x.Name.Equals("xyz", StringComparison.OrdinalIgnoreCase));

        // assert
        filteredDepartments.Should().HaveCount(0);
    }
}
