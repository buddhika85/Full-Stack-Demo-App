using Emp.Core.Entities;
using Emp.Infrastructure.Data;
using Emp.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;



namespace Emp.XUnitTests.Repositories;

public class EmployeeRepositoryTests
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
    public async Task GetAllAsync_ReturnsAllEmployees_WhenCalled()
    {
        // arrange
        var dbContext = await GetInMemoryDbContext("GetAllAsync_ReturnsAllEmployees_WhenCalled");
        var employeeRepository = new EmployeeRepository(dbContext);

        // act
        var result = await employeeRepository.GetAllAsync();

        // assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);       // this we know from seeding we did in line 25
        var johnEmployee = result.FirstOrDefault(x => x.Email.Equals("john.doe@example.com"));          // know due to seed data
        johnEmployee.Should().NotBeNull();
        johnEmployee.FirstName.Should().Be("John");
        johnEmployee.LastName.Should().Be("Doe");
        johnEmployee.DepartmentId.Should().Be(2);
        johnEmployee.Department.Should().BeNull();    // navigational properties are not loaded
    }


    [Theory]
    [InlineData(1, "John", "Doe", "john.doe@example.com", 2)]
    [InlineData(2, "Jane", "Smith", "jane.smith@example.com", 1)]
    [InlineData(3, "Peter", "Jones", "peter.jones@example.com", 2)]
    public async Task GetByIdAsync_ReturnsEmployeeById_WhenEmployeeAvailableById(int id, string firstName, string lastName, string email, int deptId)
    {
        // arrange 
        var inMemoryDb = await GetInMemoryDbContext(Guid.NewGuid().ToString());
        var repository = new EmployeeRepository(inMemoryDb);


        // act
        var result = await repository.GetByIdAsync(id);

        // assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Employee>();
        result.Id.Should().Be(id);
        result.FirstName.Should().BeEquivalentTo(firstName);
        result.LastName.Should().BeEquivalentTo(lastName);
        result.Email.Should().BeEquivalentTo(email);
        result.DepartmentId.Should().Be(deptId);
    }


    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    public async Task GetByIdAsync_ReturnsNull_WhenEmployeeWithIdUnAvailable(int unavailableId)
    {
        // arrange
        var dbContext = await GetInMemoryDbContext(Guid.NewGuid().ToString());
        var employeeReposistory = new EmployeeRepository(dbContext);

        // act
        var result = await employeeReposistory.GetByIdAsync(unavailableId);

        // assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("Test FN", "Test LN", "test@example.com", 2)]
    [InlineData("Test FN A", "Test LN B", "testAB@example.com", 1)]
    public async Task AddAsync_IncreasesRecordCountByOne_WhenInsertSuccess(string firstName, string lastName, string email, int deptId)
    {
        // arrange
        var dbContext = await GetInMemoryDbContext(Guid.NewGuid().ToString());
        var repository = new EmployeeRepository(dbContext);
        var employee = new Employee { FirstName = firstName, LastName = lastName, Email = email, DepartmentId = deptId };
        var countBeforeAdding = dbContext.Employees.Count();
        var empByEmailBefore = await dbContext.Employees.FirstOrDefaultAsync(x => x.Email.Equals(email));

        // act
        await repository.AddAsync(employee);
        await dbContext.SaveChangesAsync();
        var countAfterAdding = await dbContext.Employees.CountAsync();
        var empByEmailAfter = await dbContext.Employees.FirstOrDefaultAsync(x => x.Email.Equals(email));

        // assert        
        countAfterAdding.Should().Be(countBeforeAdding + 1);
        empByEmailBefore.Should().BeNull();
        empByEmailAfter.Should().NotBeNull();
        empByEmailAfter.FirstName.Should().Be(firstName);
        empByEmailAfter.LastName.Should().Be(lastName);
        empByEmailAfter.Email.Should().Be(email);
        empByEmailAfter.DepartmentId.Should().Be(deptId);
    }

    [Theory]
    [InlineData(1, "John Updated", "Doe Updated", "johnUpdated.doe@example.com", 1)]
    [InlineData(2, "Jane Updated", "Smith Updated", "janeUpdated.smith@example.com", 2)]
    [InlineData(3, "Peter Updated", "Jones Updated", "peterUpdated.jones@example.com", 1)]
    public async Task Update_UpdatesEmployeeAttributes_WhenAvailabled(int id, string firstName, string lastName, string email, int deptId)
    {
        // arrange
        var dbContext = await GetInMemoryDbContext(Guid.NewGuid().ToString());
        var repository = new EmployeeRepository(dbContext);
        var employee = dbContext.Employees.Single(x => x.Id == id);                 // we know employee with such Id exits due to seed data
        employee.FirstName = firstName;
        employee.LastName = lastName;
        employee.Email = email;
        employee.DepartmentId = deptId;

        // act
        repository.Update(employee);
        await dbContext.SaveChangesAsync();

        // assert
        var employeeUpdated = await repository.GetByIdAsync(employee.Id);
        employeeUpdated.Should().NotBeNull();
        employeeUpdated.FirstName.Should().Be(firstName);
        employeeUpdated.LastName.Should().Be(lastName);
        employeeUpdated.Email.Should().Be(email);
        employeeUpdated.DepartmentId.Should().Be(deptId);
    }
}
