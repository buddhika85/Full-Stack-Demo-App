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
    private readonly Mock<IPasswordHasherService> mockPasswordHasherService;

    private readonly UserService userService;

    public UserServiceTests()
    {
        mockUserRepository = new Mock<IUserRepository>();
        mockUnitOfWork = new Mock<IUnitOfWork>();
        mockLogger = new Mock<ILogger<UserService>>();
        mockJwtService = new Mock<IJwtService>();
        mockPasswordHasherService = new Mock<IPasswordHasherService>();

        mockUnitOfWork.Setup(x => x.UserRepository).Returns(mockUserRepository.Object);

        userService = new UserService(mockUnitOfWork.Object, mockLogger.Object, mockJwtService.Object, mockPasswordHasherService.Object);
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

    [Fact]
    public async Task CreateUserAsync_ThrowsInvalidOperationException_IfUsenameAlreadyExists()
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
        mockUserRepository.Setup(x => x.IsExistsAsync(createUserDto.Username)).ReturnsAsync(true);


        // act
        Func<Task> act = async () => await userService.CreateUserAsync(createUserDto);

        // assert
        await act.Should().ThrowAsync<InvalidOperationException>().Where(x => x.Message == $"Username '{createUserDto.Username}' already exists. Please use a unique username.");

        mockUserRepository.Verify(x => x.AddAsync(It.Is<User>(x =>
            x.Id == 0
            && x.Username.Equals(createUserDto.Username)
            && x.FirstName.Equals(createUserDto.FirstName)
            && x.LastName.Equals(createUserDto.LastName)
            && x.Role.Equals(createUserDto.Role.ToString())
            && x.IsActive
            )), Times.Never());

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to create user with username/email: {createUserDto.Username}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"User with username/email {createUserDto.Username} created successfully", Times.Never());

        mockLogger.VerifyMessage(LogLevel.Warning, $"User creation failed: Username '{createUserDto.Username}' already exists.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"User with username/email {createUserDto.Username} creation failed", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"Error creating a user with username/email {createUserDto.Username}", Times.Once());
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsTrue_WhenUsernameDoesNotChange()
    {
        // arrange
        var updateUserId = 1;
        var username = "staff@emp.com";
        var updateUserDto = new UpdateUserDto
        {
            FirstName = "Updated First Name",
            LastName = "Updated Last Name",
            Role = UserRoles.Admin,
            Username = username,
            Id = updateUserId,
            IsActive = true
        };
        var userFromRepo = new User
        {
            Id = updateUserId,
            Username = username,
            Role = UserRoles.Staff.ToString(),
            IsActive = false,
            FirstName = "Staff",
            LastName = "Member"
        };
        mockUserRepository.Setup(x => x.GetByIdAsync(It.Is<int>(x => x == updateUserId))).ReturnsAsync(userFromRepo);
        mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

        // act
        var result = await userService.UpdateUserAsync(updateUserId, updateUserDto);

        // assert
        result.Should().BeTrue();
        mockUserRepository.Verify(x => x.GetByIdAsync(It.Is<int>(x => x == updateUserId)), Times.Once());
        mockUserRepository.Verify(x => x.GetByUsernameAsync(It.Is<string>(x => x.Equals(updateUserDto.Username))), Times.Never());
        mockUserRepository.Verify(x => x.Update(It.Is<User>(x =>
            x.Id == updateUserId
            && x.Username.Equals(updateUserDto.Username)
            && x.FirstName.Equals(updateUserDto.FirstName)
            && x.LastName.Equals(updateUserDto.LastName)
            && x.Role.Equals(updateUserDto.Role.ToString())
            && x.IsActive == updateUserDto.IsActive
            )), Times.Once());

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to update a user with id {updateUserId} and username/email {updateUserDto.Username}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"User with id {updateUserId} and username/email {updateUserDto.Username} update successful", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"User with ID {updateUserId} username/email {updateUserDto.Username} was found, but no changes were applied or saved.", Times.Never());

        mockLogger.VerifyMessage(LogLevel.Error, $"User updated failed: User with id {updateUserId} unavailable", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"Error in updating User with id {updateUserId} and username/email {username}", Times.Never());

        mockLogger.VerifyMessage(LogLevel.Warning, $"User update failed: Username '{updateUserDto.Username}' already taken by another user.", Times.Never());
    }

    // update - change username to non existing username - passes
    [Fact]
    public async Task UpdateUserAsync_ReturnsTrue_WhenUsernameChangeButNewUsernameStillUnique()
    {
        // arrange
        var userIdToUpdate = 1;
        var oldUsername = "oldUsername@test.com";
        var newUsername = "newUsername@test.com";
        var updateUserDto = new UpdateUserDto
        {
            Id = userIdToUpdate,
            Username = newUsername,
            FirstName = "FN Updated",
            LastName = "LN Updated",
            Role = UserRoles.Admin,
            IsActive = true
        };
        var userToUpdate = new User
        {
            Id = userIdToUpdate,
            Username = oldUsername,
            FirstName = "first name",
            LastName = "last name",
            Role = UserRoles.Staff.ToString(),
            IsActive = false
        };
        mockUserRepository.Setup(x => x.GetByIdAsync(userIdToUpdate)).ReturnsAsync(userToUpdate);
        mockUserRepository.Setup(x => x.GetByUsernameAsync(newUsername)).ReturnsAsync((User?)null);     // no existing user with new username
        mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

        // act
        var result = await userService.UpdateUserAsync(userIdToUpdate, updateUserDto);

        // assert
        result.Should().Be(true);
        mockUserRepository.Verify(x => x.GetByIdAsync(It.Is<int>(x => x == userIdToUpdate)), Times.Once());
        mockUserRepository.Verify(x => x.GetByUsernameAsync(It.Is<string>(x => x.Equals(newUsername, StringComparison.OrdinalIgnoreCase))), Times.Once());
        mockUserRepository.Verify(x => x.Update(It.Is<User>(x =>
             x.Id == userIdToUpdate
            && x.Username.Equals(updateUserDto.Username)
            && x.FirstName.Equals(updateUserDto.FirstName)
            && x.LastName.Equals(updateUserDto.LastName)
            && x.Role.Equals(updateUserDto.Role.ToString())
            && x.IsActive == updateUserDto.IsActive
        )), Times.Once());


        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to update a user with id {userIdToUpdate} and username/email {updateUserDto.Username}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"User with id {userIdToUpdate} and username/email {updateUserDto.Username} update successful", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Warning, $"User update failed: Username '{updateUserDto.Username}' already taken by another user.", Times.Never());

        mockLogger.VerifyMessage(LogLevel.Information, $"User with ID {userIdToUpdate} username/email {updateUserDto.Username} was found, but no changes were applied or saved.", Times.Never());

        mockLogger.VerifyMessage(LogLevel.Error, $"User updated failed: User with id {userIdToUpdate} unavailable", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"Error in updating User with id {userIdToUpdate} and username/email {newUsername}", Times.Never());
    }


    // update - change username to an existing username - fails, as username already taken
    [Fact]
    public async Task UpdateUserAsync_ThrowsInvalidOpException_WhenUsernameChangeButNewUsernameAlreadyTaken()
    {
        // arrange
        var userIdToUpdate = 1;
        var alreadyTakenUsernameUserId = 2;
        var alreadyTakenUsername = "alreadyTaken@test.com";
        var updateUserDto = new UpdateUserDto
        {
            Id = userIdToUpdate,
            Username = alreadyTakenUsername,
            FirstName = "FN Updated",
            LastName = "LN Updated",
            Role = UserRoles.Admin,
            IsActive = true
        };
        var userToUpdate = new User
        {
            Id = userIdToUpdate,
            Username = "oldUsername@test.com",
            FirstName = "first name",
            LastName = "last name",
            Role = UserRoles.Staff.ToString(),
            IsActive = false
        };
        var otherUserWithSameUsername = new User
        {
            Id = alreadyTakenUsernameUserId,
            Username = alreadyTakenUsername,
            FirstName = "first name",
            LastName = "last name",
            Role = UserRoles.Staff.ToString(),
            IsActive = false
        };
        mockUserRepository.Setup(x => x.GetByIdAsync(It.Is<int>(x => x == userIdToUpdate))).ReturnsAsync(userToUpdate);
        mockUserRepository.Setup(x => x.GetByUsernameAsync(It.Is<string>(x => x.Equals(alreadyTakenUsername)))).ReturnsAsync(otherUserWithSameUsername);

        // act
        Func<Task> act = async () => await userService.UpdateUserAsync(userIdToUpdate, updateUserDto);

        // assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"Username '{alreadyTakenUsername}' is already taken by another user.");
        mockUserRepository.Verify(x => x.GetByIdAsync(It.Is<int>(x => x == userIdToUpdate)), Times.Once());
        mockUserRepository.Verify(x => x.GetByUsernameAsync(It.Is<string>(x => x.Equals(alreadyTakenUsername))), Times.Once());
        mockUserRepository.Verify(x => x.Update(It.Is<User>(x =>
            x.Id == userIdToUpdate
            && x.Username.Equals(alreadyTakenUsername)
            && x.FirstName.Equals(updateUserDto.LastName)
            && x.LastName.Equals(updateUserDto.LastName)
            && x.Role.Equals(updateUserDto.Role.ToString())
            && x.IsActive == userToUpdate.IsActive
            )), Times.Never());

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to update a user with id {userIdToUpdate} and username/email {updateUserDto.Username}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"User with id {userIdToUpdate} and username/email {updateUserDto.Username} update successful", Times.Never());

        mockLogger.VerifyMessage(LogLevel.Warning, $"User update failed: Username '{updateUserDto.Username}' already taken by another user.", Times.Once());

        mockLogger.VerifyMessage(LogLevel.Information, $"User with ID {userIdToUpdate} username/email {updateUserDto.Username} was found, but no changes were applied or saved.", Times.Never());

        mockLogger.VerifyMessage(LogLevel.Error, $"User updated failed: User with id {userIdToUpdate} unavailable", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"Error in updating User with id {userIdToUpdate} and username/email {alreadyTakenUsername}", Times.Once());
    }

    // update - GetByIdAsync returns null - fails
    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    public async Task UpdateUserAsync_ReturnsFalse_IfUserWithIdNotFound(int unavailableId)
    {
        // arrange
        var updateUserDto = new UpdateUserDto
        {
            Id = unavailableId,
            Username = "a@a.com",
            FirstName = "FN Updated",
            LastName = "LN Updated",
            Role = UserRoles.Admin,
            IsActive = true
        };
        mockUserRepository.Setup(x => x.GetByIdAsync(It.Is<int>(x => x == unavailableId))).ReturnsAsync((User?)null);

        // act
        var result = await userService.UpdateUserAsync(unavailableId, updateUserDto);

        // assert
        result.Should().BeFalse();
        mockUserRepository.Verify(x => x.GetByIdAsync(It.Is<int>(x => x == unavailableId)), Times.Once());
        mockUserRepository.Verify(x => x.GetByUsernameAsync(It.Is<string>(x => x.Equals(updateUserDto.Username))), Times.Never());
        mockUserRepository.Verify(x => x.Update(It.Is<User>(x =>
            x.Id == unavailableId
            && x.Username.Equals(updateUserDto.Username)
            && x.FirstName.Equals(updateUserDto.FirstName)
            && x.LastName.Equals(updateUserDto.LastName)
            && x.Role.Equals(updateUserDto.Role.ToString())
            && x.IsActive == updateUserDto.IsActive
            )), Times.Never());
        mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to update a user with id {unavailableId} and username/email {updateUserDto.Username}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"User updated failed: User with id {unavailableId} unavailable", Times.Once());

        mockLogger.VerifyMessage(LogLevel.Information, $"User with id {unavailableId} and username/email {updateUserDto.Username} update successful", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Warning, $"User update failed: Username '{updateUserDto.Username}' already taken by another user.", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Information, $"User with ID {unavailableId} username/email {updateUserDto.Username} was found, but no changes were applied or saved.", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"Error in updating User with id {unavailableId} and username/email {updateUserDto.Username}", Times.Never());
    }

    [Theory]
    [InlineData("test1@gmail.com", "qwe123$")]
    [InlineData("test2@gmail.com", "abc456%")]
    public async Task AuthenticateUserAsync_ReturnsJwtToken_WhenCredentialsValid(string username, string password)
    {
        // arrange
        var loginDto = new LoginDto
        {
            Password = password,
            Username = username
        };
        var user = new User
        {
            FirstName = "FN",
            LastName = "LN",
            Role = UserRoles.Staff.ToString(),
            IsActive = true,
            PasswordHash = "HashedPassword",
            Username = username
        };
        var jwtToken = "JWT-Token";
        mockUserRepository.Setup(x => x.GetByUsernameAsync(It.Is<string>(x => x.Equals(loginDto.Username)))).ReturnsAsync(user);
        mockPasswordHasherService.Setup(x => x.VerifyPassword(loginDto.Password, user.PasswordHash)).Returns(true);
        mockJwtService.Setup(x => x.GenerateJwtToken(user)).Returns(jwtToken);

        // act
        var result = await userService.AuthenticateUserAsync(loginDto);

        // assert
        result.Should().NotBeNull();
        result.Should().Be(jwtToken);

        mockUserRepository.Verify(x => x.GetByUsernameAsync(It.Is<string>(x => x.Equals(loginDto.Username))), Times.Once());
        mockPasswordHasherService.Verify(x => x.VerifyPassword(loginDto.Password, user.PasswordHash), Times.Once());
        mockJwtService.Verify(x => x.GenerateJwtToken(user), Times.Once());

        mockLogger.VerifyMessage(LogLevel.Information, $"Authenticating user - {loginDto.Username}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"User authetication failed: User with Username {loginDto.Username} unavailable", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"User authetication failed: User with Username {loginDto.Username} is not active", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"User authetication failed: Password does not match for user {loginDto.Username}", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Information, $"User authetication success: For user {loginDto.Username}. Now generating JWT token.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"JWT token generation success: For user {loginDto.Username}.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"Error in autheticating User with id {loginDto.Username}", Times.Never());
    }

    // user with username not found - returns null
    // user inactive - returns null
    // password verifucation failed - returns null
    // exception occurs, catch bock logs and bubbles up exception to caller
}
