
using Emp.Api.Controllers;
using Emp.Core.DTOs;
using Emp.Core.Entities;
using Emp.Core.Interfaces.Services;
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
                Role = Core.Enums.UserRoles.Staff,
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
    }
}
