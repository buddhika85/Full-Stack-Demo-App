using Emp.Application.Services;
using Emp.Core;
using Emp.Core.Entities;
using Emp.Core.Extensions;
using Emp.Core.Interfaces.Repositories;
using Emp.Core.Interfaces.Services;
using Emp.XUnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Emp.XUnitTests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> mockEmployeeRepo;
    private readonly Mock<IDepartmentRepository> mockDepartmentRepo;
    private readonly Mock<IUserRepository> mockUserRepo;

    private readonly Mock<IUnitOfWork> mockUnitOfWork;
    private readonly Mock<ILogger<EmployeeService>> mockLogger;
    private readonly IEmployeeService employeeService;

    public EmployeeServiceTests()
    {
        mockEmployeeRepo = new Mock<IEmployeeRepository>();
        mockDepartmentRepo = new Mock<IDepartmentRepository>();
        mockUserRepo = new Mock<IUserRepository>();
        mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(x => x.EmployeeRepository).Returns(mockEmployeeRepo.Object);
        mockUnitOfWork.Setup(x => x.DepartmentRepository).Returns(mockDepartmentRepo.Object);
        mockUnitOfWork.Setup(x => x.UserRepository).Returns(mockUserRepo.Object);

        mockLogger = new Mock<ILogger<EmployeeService>>();
        employeeService = new EmployeeService(mockUnitOfWork.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetAllEmployeesAsync_ReturnsAllEmployees_WhenCalled()
    {
        // arrange
        var allEmps = new List<Employee>
        {
            new Employee { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", DepartmentId = 2 },
            new Employee { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", DepartmentId = 1 },
            new Employee { Id = 3, FirstName = "Peter", LastName = "Jones", Email = "peter.jones@example.com", DepartmentId = 2 }
        };
        mockEmployeeRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(allEmps);

        // act
        var result = await employeeService.GetAllEmployeesAsync();

        // assert        
        result.Should().NotBeNull();
        result.Count().Should().Be(allEmps.Count);
        var empTwo = result.SingleOrDefault(x => x.Id == 2);
        empTwo.Should().NotBeNull();
        empTwo.Should().BeEquivalentTo(allEmps.Single(x => x.Id == 2).ToDto());

        mockEmployeeRepo.Verify(x => x.GetAllAsync(), Times.Once());

        mockLogger.VerifyMessage(LogLevel.Information, "Attempting to get all employees", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"Successfully retrieved {allEmps.Count()} employees.", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, "Error retrieving all employees.", Times.Never());
    }
}
