
using Emp.Api.Controllers;
using Emp.Core.DTOs;
using Emp.Core.Entities;
using Emp.Core.Enums;
using Emp.Core.Interfaces.Services;
using Emp.XUnitTests.Helpers;
using Emp.XUnitTests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Emp.XUnitTests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> mockUserService;
    private readonly Mock<ILogger<UsersController>> mockLogger;
    private readonly UsersController usersController;

    public UsersControllerTests()
    {
        mockUserService = new Mock<IUserService>();
        mockLogger = new Mock<ILogger<UsersController>>();
        usersController = new UsersController(mockUserService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetUsers_ReturnsAllUsers_WhenCalled()
    {
        // arrange 
        IEnumerable<UserDto> testUsers = new List<UserDto> {
            new UserDto
            {
                FirstName = "FirstName",
                LastName = "LastName",
                Role = UserRoles.Staff,
                Username = "user@user.com",
                Id = 1,
                IsActive = true
            }
        };
        mockUserService.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(testUsers);

        // act
        var result = await usersController.GetUsers();

        // assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ActionResult<IEnumerable<UserDto>>>();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        returnedUsers.Should().NotBeNull();
        returnedUsers.Should().HaveCount(testUsers.Count());
        returnedUsers.Should().BeEquivalentTo(testUsers);

        mockLogger.VerifyMessage(LogLevel.Information, "API: GetUsers endpoint called by Admin.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"API: Error in GetUsers endpoint.", Times.Never());

        mockUserService.Verify(x => x.GetAllUsersAsync(), Times.Once());
    }

    [Theory]
    [ClassData(typeof(UserTestData))]
    public async Task GetUser_ReturnsCorrectUser_WhenUserAvailableWithId(User user)
    {
        // arrange
        var testUserDto = new UserDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Id = user.Id,
            IsActive = user.IsActive,
            Role = Enum.Parse<UserRoles>(user.Role),
            Username = user.Username,
        };
        mockUserService.Setup(x => x.GetUserByIdAsync(user.Id)).ReturnsAsync(testUserDto);

        // act
        var result = await usersController.GetUser(user.Id);

        // assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ActionResult<UserDto>>();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var userReturned = okResult.Value.Should().BeAssignableTo<UserDto>().Subject;
        userReturned.Should().NotBeNull();
        userReturned.Should().BeEquivalentTo(testUserDto);

        mockLogger.VerifyMessage(LogLevel.Information, $"API: GetUser endpoint called for ID: {user.Id} by Admin.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Warning, $"API: User with ID {user.Id} not found.", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"API: Error in GetUser endpoint for ID: {user.Id}.", Times.Never());

        mockUserService.Verify(x => x.GetUserByIdAsync(It.Is<int>(id => id == user.Id)), Times.Once());
    }
}
