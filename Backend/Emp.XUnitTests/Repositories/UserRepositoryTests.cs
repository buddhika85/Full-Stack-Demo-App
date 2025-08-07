using Emp.Core.Entities;
using Emp.Core.Enums;
using Emp.Infrastructure.Data;
using Emp.Infrastructure.Repositories;
using Emp.XUnitTests.TestData;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Emp.XUnitTests.Repositories;

public class UserRepositoryTests
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
    public async Task GetAllAsync_ReturnsAllUsers_WhenCalled()
    {
        // arrange
        var testDbContext = await GetInMemoryDbContext("GetAllAsync_ReturnsAllUsers_WhenCalled");
        var repository = new UserRepository(testDbContext);

        // act        
        var usersActual = await repository.GetAllAsync();

        // assert
        usersActual.Should().NotBeNull();
        usersActual.Should().HaveCount(2);                                          // DbContext seeds 2 users
        usersActual.Should().Contain(x => x.Username == "admin@emp.com");           // as per seeding
        usersActual.Should().Contain(x => x.Username == "staff@emp.com");
        usersActual.Should().Contain(x => x.Role == nameof(UserRoles.Admin));
        usersActual.Should().Contain(x => x.Role == nameof(UserRoles.Staff));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetByIdAsync_RetunrsCorrectUser_ForExistingIdPassed(int userId)
    {
        // arrange
        var testDbContext = await GetInMemoryDbContext("GetByIdAsync_RetunrsCorrectUser_ForExistingIdPassed");
        var repository = new UserRepository(testDbContext);

        // act
        var actual = await repository.GetByIdAsync(userId);

        // assert
        actual.Should().NotBeNull();
        actual.Id.Should().Be(userId);
    }

    [Theory]
    [ClassData(typeof(UserTestData))]
    public async Task GetByIdAsync_RetunrsCorrectUser_ForExistingIdsPassed(User user)
    {
        // arrange
        var testDbContext = await GetInMemoryDbContext("GetByIdAsync_RetunrsCorrectUser_ForExistingIdsPassed");
        var repository = new UserRepository(testDbContext);

        // act
        var actual = await repository.GetByIdAsync(user.Id);

        // assert
        actual.Should().NotBeNull();
        actual.Id.Should().Be(user.Id);
        actual.Username.Should().Be(user.Username);
        actual.Role.Should().Be(user.Role);
        actual.IsActive.Should().Be(user.IsActive);
        actual.FirstName.Should().Be(user.FirstName);
        actual.LastName.Should().Be(user.LastName);
    }
}
