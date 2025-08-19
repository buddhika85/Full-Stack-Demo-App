using Emp.Application.Services;
using Emp.Core;
using Emp.Core.DTOs;
using Emp.Core.Entities;
using Emp.Core.Enums;
using Emp.Core.Interfaces.Repositories;
using Emp.Core.Interfaces.Services;
using Emp.XUnitTests.Helpers;
using Emp.XUnitTests.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Emp.XUnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> mockUserRepository;
    private readonly Mock<IUnitOfWork> mockUnitOfWork;
    private readonly Mock<ILogger<UserService>> mockLogger;
    private readonly Mock<IJwtService> mockJwtService;

    private readonly UserService userService;

    public UserServiceTests()
    {
        mockUserRepository = new Mock<IUserRepository>();
        mockUnitOfWork = new Mock<IUnitOfWork>();
        mockLogger = new Mock<ILogger<UserService>>();
        mockJwtService = new Mock<IJwtService>();

        mockUnitOfWork.Setup(x => x.UserRepository).Returns(mockUserRepository.Object);

        userService = new UserService(mockUnitOfWork.Object, mockLogger.Object, mockJwtService.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers_WhenCalled()
    {
        // arrange
        IEnumerable<User> testUsers = new List<User> {
            new User
            {
                FirstName = "FirstName",
                LastName = "LastName",
                Role = "Staff",
                Username = "user@user.com",
                Id = 1,
                IsActive = true
            }
        };
        mockUserRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(testUsers);

        // act
        var result = await userService.GetAllUsersAsync();

        // assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Count().Should().Be(testUsers.Count());
        result.First().Should().BeEquivalentTo(new UserDto
        {
            FirstName = "FirstName",
            LastName = "LastName",
            Role = Core.Enums.UserRoles.Staff,
            Username = "user@user.com",
            Id = 1,
            IsActive = true
        });

        mockLogger.VerifyMessage(LogLevel.Information, "Attempting to get all users", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"Successfully retrieved {testUsers.Count()} users.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"Error in getting all users.", Times.Never());

        mockUserRepository.Verify(x => x.GetAllAsync(), Times.Once());
    }

    [Theory]
    [ClassData(typeof(UserTestData))]
    public async Task GetUserByIdAsync_ReturnsUser_WhenExists(User user)
    {
        // arrange 
        mockUserRepository.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);

        // act
        UserDto? result = await userService.GetUserByIdAsync(user.Id);

        // assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.IsActive.Should().Be(user.IsActive);

        Enum.TryParse(user.Role, out UserRoles userRole);
        result.Role.Should().Be(userRole);

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to get a user by id {user.Id}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"User with Id {user.Id} retrieved.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Warning, $"User with id {user.Id} unavailable", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"Error in getting user by id {user.Id}", Times.Never());

        mockUserRepository.Verify(x => x.GetByIdAsync(It.Is<int>(id => id == user.Id)), Times.Once());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public async Task GetUserByIdAsync_ReturnsNull_WhenUserWithIdUnavailable(int unavilableUserId)
    {
        // arrange
        User? nullUser = null;
        mockUserRepository.Setup(x => x.GetByIdAsync(unavilableUserId)).ReturnsAsync(nullUser);

        // act
        var result = await userService.GetUserByIdAsync(unavilableUserId);

        // assert
        result.Should().BeNull();
        result.Should().BeEquivalentTo(nullUser);

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to get a user by id {unavilableUserId}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Warning, $"User with id {unavilableUserId} unavailable", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"User with Id {unavilableUserId} retrieved.", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Information, $"Error in getting user by id {unavilableUserId}", Times.Never());

        mockUserRepository.Verify(x => x.GetByIdAsync(It.Is<int>(id => id == unavilableUserId)), Times.Once());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetUserByIdAsync_ThrowsException_WhenGetByIdAsyncThrows(int id)
    {
        // arrange
        mockUserRepository.Setup(x => x.GetByIdAsync(id)).ThrowsAsync(new Exception { Source = "Test Exception" });

        // act
        Func<Task> act = async () => await userService.GetUserByIdAsync(id);

        // asert
        await act.Should().ThrowAsync<Exception>().Where(x => x.Source == "Test Exception");

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to get a user by id {id}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Warning, $"User with id {id} unavailable", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Information, $"User with Id {id} retrieved.", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"Error in getting user by id {id}", Times.Once());
    }


    [Fact]
    public async Task CreateUserAsync_ReturnsNewUserDto_IfUsenameIsUnique()
    {
        // arrange 
        var createUserDto = new CreateUserDto
        {
            FirstName = "Test FN",
            LastName = "Test LN",
            Password = "TestPw456#",
            Username = "test@test.com",
            Role = UserRoles.Staff
        };
        var expected = new UserDto
        {
            Id = 0,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Role = createUserDto.Role,
            Username = createUserDto.Username,
            IsActive = true,
        };
        mockUserRepository.Setup(x => x.IsExistsAsync(createUserDto.Username)).ReturnsAsync(false);
        mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

        // act
        var result = await userService.CreateUserAsync(createUserDto);

        // assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);

        mockUserRepository.Verify(x => x.AddAsync(It.Is<User>(x =>
            x.Id == 0
            && x.Username.Equals(createUserDto.Username)
            && x.FirstName.Equals(createUserDto.FirstName)
            && x.LastName.Equals(createUserDto.LastName)
            && x.Role.Equals(createUserDto.Role.ToString())
            && x.IsActive
            )), Times.Once());

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to create user with username/email: {createUserDto.Username}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"User with username/email {createUserDto.Username} created successfully", Times.Once());

        mockLogger.VerifyMessage(LogLevel.Warning, $"User creation failed: Username '{createUserDto.Username}' already exists.", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"User with username/email {createUserDto.Username} creation failed", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"Error creating a user with username/email {createUserDto.Username}", Times.Never());
    }
}
