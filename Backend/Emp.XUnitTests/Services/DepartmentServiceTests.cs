using Emp.Application.Services;
using Emp.Core;
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
        var departments = new List<Department>
            {
                new Department { Id = 1, Name = "HR" },
                new Department { Id = 2, Name = "IT" }
            };
        mockDepartmentRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(departments);

        // act
        var result = await departmentService.GetAllDepartmentsAsync();

        // assert
        result.Should().NotBeNull();
        result.Should().HaveCount(departments.Count());

        // asserting on log messages
        mockLogger.VerifyMessage(LogLevel.Information, "Attempting to get all departments", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"Retrived {departments.Count()} departments", Times.Once());
    }



}
