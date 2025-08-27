using Emp.Api.Controllers;
using Emp.Core.DTOs;
using Emp.Core.Entities;
using Emp.Core.Enums;
using Emp.Core.Interfaces.Services;
using Emp.XUnitTests.Helpers;
using Emp.XUnitTests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public async Task GetUser_ReturnsNotFoundError_WhenUserWithIdUnavailable(int unavailableId)
    {
        // arrange
        mockUserService.Setup(x => x.GetUserByIdAsync(unavailableId)).ReturnsAsync((UserDto?)null);

        // act
        var result = await usersController.GetUser(unavailableId);

        // assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ActionResult<UserDto>>();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Should().NotBeNull();
        notFoundResult.StatusCode.Should().Be(404);

        var problemDetils = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetils.Title.Should().Be("Not Found");
        problemDetils.Detail.Should().Be($"API: User with ID {unavailableId} not found.");
        problemDetils.Status.Should().Be(StatusCodes.Status404NotFound);

        mockLogger.VerifyMessage(LogLevel.Information, $"API: GetUser endpoint called for ID: {unavailableId} by Admin.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Warning, $"API: User with ID {unavailableId} not found.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"API: Error in GetUser endpoint for ID: {unavailableId}.", Times.Never());

        mockUserService.Verify(x => x.GetUserByIdAsync(It.Is<int>(id => id == unavailableId)), Times.Once());
    }

    //[Fact]
    //public async Task GetUser_ReturnsInternalServerError_WhenAnExceptionThrown()
    //{
    //    // arrange 

    //    // act

    //    // assert
    //}


    [Theory]
    [InlineData("test@test.com", "qwe123$", "Test Fn", "Test Ln", UserRoles.Staff)]
    [InlineData("test@test.com", "qwe123$", "Test Fn", "Test Ln", UserRoles.Admin)]
    public async Task CreateUser_ReturnsUserDto_WhenValidDataPassed
        (string username, string password, string firstName, string lastName, UserRoles role)
    {
        // arrange
        var createUserDto = new CreateUserDto
        {
            FirstName = firstName,
            LastName = lastName,
            Password = password,
            Username = username,
            Role = role
        };
        var expectedUserDto = new UserDto
        {
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            Username = username,
            Id = 0,
            IsActive = true
        };
        mockUserService.Setup(x => x.CreateUserAsync(It.Is<CreateUserDto>(x => x == createUserDto))).ReturnsAsync(expectedUserDto);

        // act
        var result = await usersController.CreateUser(createUserDto);

        // assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ActionResult<UserDto>>();

        var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdAtActionResult.ActionName.Should().Be("GetUser");
        createdAtActionResult.RouteValues.Should().HaveCount(1);
        createdAtActionResult.RouteValues["id"].Should().Be(expectedUserDto.Id);

        createdAtActionResult.Value.Should().NotBeNull();
        var userDto = createdAtActionResult.Value.Should().BeOfType<UserDto>().Subject;
        userDto.Should().BeEquivalentTo(expectedUserDto);

        mockLogger.VerifyMessage(LogLevel.Information, $"API: CreateUser endpoint called for username: {createUserDto.Username} by Admin.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"API: User '{expectedUserDto.Username}' created successfully with ID: {expectedUserDto.Id} by Admin.", Times.Once());

        mockLogger.VerifyMessage(LogLevel.Warning, "API: CreateUser validation failed. Errors:", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"API: New user creation failed for username {createUserDto.Username}", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"API: Error in CreateUser endpoint for username: {createUserDto.Username}.", Times.Never());
    }

    [Theory]
    [InlineData("test@test.com", "qwe123$", "Test Fn", "Test Ln", UserRoles.Staff)]
    [InlineData("test@test.com", "qwe123$", "Test Fn", "Test Ln", UserRoles.Admin)]
    public async Task CreateUser_ReturnsInternalServerError_WhenInvalidOperationExceptionThrown
        (string username, string password, string firstName, string lastName, UserRoles role)
    {
        // arrange
        var createUserDto = new CreateUserDto
        {
            FirstName = firstName,
            LastName = lastName,
            Password = password,
            Username = username,
            Role = role
        };
        var testExceptionMessage = "Test Invalid Operation Exception";
        mockUserService.Setup(x => x.CreateUserAsync(It.Is<CreateUserDto>(x => x == createUserDto))).ThrowsAsync(new InvalidOperationException(testExceptionMessage));

        // act
        var result = await usersController.CreateUser(createUserDto);

        // assert
        result.Should().NotBeNull();
        var internalServerError = result.Result.Should().BeOfType<ObjectResult>().Subject;
        internalServerError.Should().NotBeNull();
        internalServerError.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var problemDetails = internalServerError.Value.Should().BeAssignableTo<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be($"API: Error in CreateUser endpoint for username: {createUserDto.Username}.");
        problemDetails.Detail.Should().Be(testExceptionMessage);
        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);

        mockUserService.Verify(x => x.CreateUserAsync(It.Is<CreateUserDto>(x => x == createUserDto)), Times.Once());

        mockLogger.VerifyMessage(LogLevel.Information, $"API: CreateUser endpoint called for username: {createUserDto.Username} by Admin.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"API: User '{createUserDto.Username}' created successfully with ID: 0 by Admin.", Times.Never());

        mockLogger.VerifyMessage(LogLevel.Warning, "API: CreateUser validation failed. Errors:", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"API: New user creation failed for username {createUserDto.Username}", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"API: Error in CreateUser endpoint for username: {createUserDto.Username}.", Times.Once());
    }


    [Theory]
    [ClassData(typeof(UserTestData))]
    public async Task CreateUser_ReturnsInternalServerError_WhenServiceReturnsNull(User user)
    {
        // arrnage 
        var createUserDto = new CreateUserDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Password = "qwe123$",
            Username = user.Username,
            Role = Enum.Parse<UserRoles>(user.Role),
        };
        mockUserService.Setup(x => x.CreateUserAsync(It.Is<CreateUserDto>(x =>
                x.Username.Equals(createUserDto.Username)
                && x.FirstName.Equals(createUserDto.FirstName)
                && x.LastName.Equals(createUserDto.LastName)
                && x.Password.Equals(createUserDto.Password)
                && x.Role == createUserDto.Role
            )))
            .ReturnsAsync((UserDto?)null);

        // act 
        var result = await usersController.CreateUser(createUserDto);

        // assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ActionResult<UserDto>>();

        var internalServerError = result.Result.Should().BeOfType<ObjectResult>().Subject;
        internalServerError.Should().NotBeNull();
        internalServerError.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var problemDetails = internalServerError.Value.Should().BeAssignableTo<ProblemDetails>().Subject;
        problemDetails.Should().NotBeNull();
        problemDetails.Title.Should().Be("Internal Server Error");
        problemDetails.Detail.Should().Be($"New user creation failed for username {createUserDto.Username}");
        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);

        mockUserService.Verify(x => x.CreateUserAsync(It.Is<CreateUserDto>(x =>
                x.Username.Equals(createUserDto.Username)
                && x.FirstName.Equals(createUserDto.FirstName)
                && x.LastName.Equals(createUserDto.LastName)
                && x.Password.Equals(createUserDto.Password)
                && x.Role == createUserDto.Role)), Times.Once());

        mockLogger.VerifyMessage(LogLevel.Information, $"API: CreateUser endpoint called for username: {createUserDto.Username} by Admin.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"API: User '{createUserDto.Username}' created successfully with ID: 0 by Admin.", Times.Never());

        mockLogger.VerifyMessage(LogLevel.Warning, "API: CreateUser validation failed. Errors:", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"API: New user creation failed for username {createUserDto.Username}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"API: Error in CreateUser endpoint for username: {createUserDto.Username}.", Times.Never());
    }


    [Theory]
    [InlineData("invalid email 1", "qwe123$", "Test Fn", "Test Ln", UserRoles.Staff)]
    [InlineData("invalid email 2", "qwe123$", "Test Fn", "Test Ln", UserRoles.Admin)]
    public async Task CreateUser_ReturnsBadRequestResult_WhenEmailIsInvalid
            (string invalidEmail, string password, string firstName, string lastName, UserRoles role)
    {
        // arrange
        var createDtoWithInvalidEmail = new CreateUserDto
        {
            FirstName = firstName,
            LastName = lastName,
            Password = password,
            Role = role,
            Username = invalidEmail
        };
        Helpers.Helpers.ApplyModelStateErrors(createDtoWithInvalidEmail, usersController);          // In Unit Testing you need to programatically apply model state errors


        // act
        var result = await usersController.CreateUser(createDtoWithInvalidEmail);

        // assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ActionResult<UserDto>>();

        var badRequestObjectResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestObjectResult.Should().NotBeNull();

        var problemDetails = badRequestObjectResult.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        problemDetails.Should().NotBeNull();

        var modelStateError = problemDetails.Errors["Validation Errors"].FirstOrDefault();
        modelStateError.Should().NotBeNull();
        modelStateError.Should().Be("The Username field is not a valid e-mail address.");

        mockUserService.Verify(x => x.CreateUserAsync(It.Is<CreateUserDto>(x =>
                            x.Username.Equals(invalidEmail)
                            && x.FirstName.Equals(createDtoWithInvalidEmail.FirstName)
                            && x.LastName.Equals(createDtoWithInvalidEmail.LastName)
                            && x.Password.Equals(createDtoWithInvalidEmail.Password)
                            && x.Role == createDtoWithInvalidEmail.Role
                            )), Times.Never());

        mockLogger.VerifyMessage(LogLevel.Information, $"API: CreateUser endpoint called for username: {invalidEmail} by Admin.", Times.Once());


        mockLogger.VerifyMessage(LogLevel.Warning, "API: CreateUser validation failed. Errors:", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"API: New user creation failed for username {invalidEmail}", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, $"API: Error in CreateUser endpoint for username: {invalidEmail}.", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Information, $"API: User '{invalidEmail}' created successfully with ID: 0 by Admin.", Times.Never());
    }
}
