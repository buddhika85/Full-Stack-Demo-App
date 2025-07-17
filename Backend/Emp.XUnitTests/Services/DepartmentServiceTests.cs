using Emp.Application.Services;
using Emp.Core;
using Emp.Core.DTOs;
using Emp.Core.Entities;
using Emp.Core.Interfaces.Repositories;
using Emp.Core.Interfaces.Services;
using Emp.XUnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Emp.XUnitTests.Services;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> mockDepartmentRepository;
    private readonly Mock<IEmployeeRepository> mockEmployeeRepository;
    private readonly Mock<IUnitOfWork> mockUnitOfWork;

    private readonly Mock<ILogger<DepartmentService>> mockLogger;
    private readonly IDepartmentService departmentService;

    public DepartmentServiceTests()
    {
        mockDepartmentRepository = new Mock<IDepartmentRepository>();
        mockEmployeeRepository = new Mock<IEmployeeRepository>();

        mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(x => x.DepartmentRepository).Returns(mockDepartmentRepository.Object);
        mockUnitOfWork.Setup(x => x.EmployeeRepository).Returns(mockEmployeeRepository.Object);

        mockLogger = new Mock<ILogger<DepartmentService>>();
        departmentService = new DepartmentService(mockUnitOfWork.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_GetsAllDepartments_WhenCalled()
    {
        // arrange
        var testData = new List<Department>
            {
                new Department { Id = 1, Name = "HR" },
                new Department { Id = 2, Name = "IT" }
            };
        mockDepartmentRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(testData);

        // act
        var departments = await departmentService.GetAllDepartmentsAsync();

        // assert
        departments.Should().NotBeNull();
        departments.Should().HaveCount(testData.Count());

        // asserting on log messages
        mockLogger.VerifyMessage(LogLevel.Information, "Attempting to get all departments", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"Retrived {testData.Count()} departments", Times.Once());
    }

    [Theory]
    [InlineData(1, "HR")]
    [InlineData(100, "Engineering")]
    [InlineData(10001, "IT")]
    public async Task GetDepartmentByIdAsync_ReturnsDepartment_WhenCalledWithExistingId(int deptId, string deptName)
    {
        // arrange       
        mockDepartmentRepository.Setup(x => x.GetByIdAsync(deptId)).ReturnsAsync(new Department { Id = deptId, Name = deptName });

        // act
        var departmentDto = await departmentService.GetDepartmentByIdAsync(deptId);

        // assert
        departmentDto.Should().NotBeNull();
        departmentDto.Id.Should().Be(deptId);
        departmentDto.Name.Should().Be(deptName);
        mockLogger.VerifyMessage(LogLevel.Information, $"Atempting to get a department with ID {deptId}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"Department with id {deptId} retrieved", Times.Once());
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_ReturnsNull_WhenCalledWithNonExistentId()
    {
        // arrange       
        var nonExitentId = 100;
        Department? nullDepartment = null;
        mockDepartmentRepository.Setup(x => x.GetByIdAsync(nonExitentId)).ReturnsAsync(nullDepartment);

        // act
        var department = await departmentService.GetDepartmentByIdAsync(nonExitentId);

        // assert
        department.Should().BeNull();
        mockLogger.VerifyMessage(LogLevel.Information, $"Atempting to get a department with ID {nonExitentId}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Warning, $"Department with id {nonExitentId} unavailable", Times.Once());
    }

    [Theory]
    [InlineData("HR")]
    [InlineData("Engineering")]
    [InlineData("IT")]
    public async Task CreateDepartmentAsync_ReturnsCreatedDepartment_WhenCalledWithValidDepartment(string deptName)
    {
        // arrange       
        var createDepartmentDto = new CreateDepartmentDto { Name = deptName };
        mockDepartmentRepository.Setup(x => x.AddAsync(new Department { Name = deptName })).Returns(Task.CompletedTask);
        mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

        // act
        var departmentCreated = await departmentService.CreateDepartmentAsync(createDepartmentDto);

        // assert
        departmentCreated.Should().NotBeNull();
        departmentCreated.Name.Should().Be(deptName);
        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to create department with name: {deptName}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"Department '{deptName}' (ID: 0) created successfully.", Times.Once());
    }
}
