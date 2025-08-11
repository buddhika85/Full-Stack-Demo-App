using Emp.Application.Services;
using Emp.Core;
using Emp.Core.DTOs;
using Emp.Core.Entities;
using Emp.Core.Interfaces.Repositories;
using Emp.Core.Interfaces.Services;
using Emp.Infrastructure.Repositories;
using Emp.XUnitTests.Helpers;
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
}
