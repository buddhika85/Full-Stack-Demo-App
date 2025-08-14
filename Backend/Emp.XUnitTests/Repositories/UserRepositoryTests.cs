using Emp.Core.Entities;
using Emp.Core.Enums;
using Emp.Infrastructure.Data;
using Emp.Infrastructure.Repositories;
using Emp.XUnitTests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
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


    [Theory]
    [InlineData(1000)]
    [InlineData(2000)]
    public async Task GetByIdAsync_RetunrsNull_ForNonExistingIdPassed(int userId)
    {
        // arrange
        var testDbContext = await GetInMemoryDbContext("GetByIdAsync_RetunrsCorrectUser_ForExistingIdPassed");
        var repository = new UserRepository(testDbContext);

        // act
        var actual = await repository.GetByIdAsync(userId);

        // assert
        actual.Should().BeNull();
    }


    [Fact]
    public async Task AddAsync_AddsRecord_WhenNewUserPassed()
    {
        // arrange
        var testDbContext = await GetInMemoryDbContext("AddAsync_AddsRecord_WhenNewUserPassed");
        var repository = new UserRepository(testDbContext);
        var testUser = new User
        {
            FirstName = "Test FN",
            LastName = "Test LN",
            Role = nameof(UserRoles.Staff),
            IsActive = true,
            Username = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123")
        };
        var empCount = testDbContext.Users.Count();

        // act
        await repository.AddAsync(testUser);
        await testDbContext.SaveChangesAsync();


        var empCountAfterAdd = testDbContext.Employees.Count();
        var newUser = await repository.GetByIdAsync(testUser.Id);

        // assert
        empCountAfterAdd.Should().Be(empCount + 1);
        newUser.Should().NotBeNull();
        newUser.FirstName.Should().Be(testUser.FirstName);
        newUser.LastName.Should().Be(testUser.LastName);
        newUser.Role.Should().Be(testUser.Role);
        newUser.Username.Should().Be(testUser.Username);
        newUser.PasswordHash.Should().Be(testUser.PasswordHash);
    }



    [Theory]
    [InlineData(1)]
    public async Task Update_UpdatesUser_WhenUserToUpdatePassed(int userId)
    {
        // arrange
        var testDbContext = await GetInMemoryDbContext("GetAllAsync_ThrowsException_WhenGenericRepositoryThrowsException");
        var userRepository = new UserRepository(testDbContext);

        var testUser = await testDbContext.Users.FindAsync(userId);
        testUser.Should().NotBeNull();

        testUser.FirstName = $"FN updated";
        testUser.LastName = $"LN updated";
        testUser.Role = nameof(UserRoles.Staff);
        testUser.IsActive = true;
        testUser.Username = "updateUn@test.com";
        testUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("UpdatedTest@123");



        // act
        var userCountBeforeUpdate = testDbContext.Users.Count();
        userRepository.Update(testUser);
        await testDbContext.SaveChangesAsync();

        // assert       
        testDbContext.Users.Count().Should().Be(userCountBeforeUpdate);
        var updatedUser = await testDbContext.Users.FindAsync(userId);
        updatedUser.Should().NotBeNull();
        updatedUser.Should().BeEquivalentTo(testUser);
    }

}
