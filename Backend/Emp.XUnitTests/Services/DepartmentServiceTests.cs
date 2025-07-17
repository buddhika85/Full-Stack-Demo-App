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

        // verify repository GetAllAsync was called once
        mockDepartmentRepository.Verify(x => x.GetAllAsync(), Times.Once());
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

        // verify repository GetByIdAsync was called once
        mockDepartmentRepository.Verify(x => x.GetByIdAsync(It.Is<int>(id => id == deptId)), Times.Once());
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

        // verify repository GetByIdAsync was called once
        mockDepartmentRepository.Verify(x =>
                                            x.GetByIdAsync(It.Is<int>(id => id == nonExitentId)),
                                            Times.Once());
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

        // verify repository addAsync was called once
        mockDepartmentRepository.Verify(x => x.AddAsync(It.Is<Department>(d => d.Name == deptName)), Times.Once);
        // verify UOW completeAsync was called once
        mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to create department with name: {deptName}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"Department '{deptName}' (ID: 0) created successfully.", Times.Once());
    }

    [Theory]
    [InlineData(100, "HR", "Human Resources")]
    [InlineData(1011, "IT", "Information Technology")]
    public async Task UpdateDepartmentAsync_ReturnsTrue_IfUpdateSuccess(int id, string name, string updatedName)
    {
        // arrange
        var updateDto = new UpdateDepartmentDto { Id = id, Name = updatedName };
        var entityToUpdate = new Department { Id = id, Name = name };
        mockDepartmentRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entityToUpdate);
        mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

        // act
        var status = await departmentService.UpdateDepartmentAsync(id, updateDto);

        // assert
        status.Should().BeTrue();
        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to update an department with ID {id}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"Department with ID {id} updated successfully.", Times.Once());

        // verify 
        mockDepartmentRepository.Verify(x => x.Update(It.Is<Department>(x => x.Id == id && x.Name == updatedName)), Times.Once());          // repository Update was called once
        mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());        // UOW complete async called once
    }

    [Fact]
    public async Task UpdateDepartmentAsync_ReturnsFalse_IfDepartmentNonExistent()
    {
        // arrange
        var nonExistentId = 99;
        var updatedName = "updated name";
        Department? departmentNullResult = null;
        mockDepartmentRepository.Setup(x => x.GetByIdAsync(nonExistentId)).ReturnsAsync(departmentNullResult);

        // act
        var status = await departmentService.UpdateDepartmentAsync(nonExistentId, new UpdateDepartmentDto { Id = nonExistentId, Name = updatedName });

        // assert
        status.Should().BeFalse();
        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to update an department with ID {nonExistentId}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Warning, $"Updated Failed: Department with id {nonExistentId} unavailable", Times.Once());

        // verify never called
        mockDepartmentRepository.Verify(x => x.Update(It.Is<Department>(x => x.Id == nonExistentId && x.Name == updatedName)), Times.Never());          // repository Update was never called
        mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());        // UOW complete async was never called
    }

    [Theory]
    [InlineData(1, "HR")]
    [InlineData(101, "IT")]
    public async Task UpdateDepartmentAsync_ReturnsFalse_IfNoRowsAffected(int id, string name)
    {
        // arrange       
        var entityToUpdate = new Department { Id = id, Name = name };
        mockDepartmentRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entityToUpdate);
        mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(0);                           // set up to return 0 - EF - no rows effected on DB

        // act
        var status = await departmentService.UpdateDepartmentAsync(id, new UpdateDepartmentDto { Id = id, Name = name });

        // assert
        status.Should().BeFalse();
        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to update an department with ID {id}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"Department with ID {id} update unsuccessful.", Times.Once());

        // verify 
        mockDepartmentRepository.Verify(x => x.Update(It.Is<Department>(x => x.Id == id && x.Name == name)), Times.Once());          // repository Update was called once
        mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());        // UOW complete async called once
    }


    [Theory]
    [InlineData(100, "HR")]
    [InlineData(1011, "IT")]
    public async Task DeleteDepartmentAsync_ReturnsTrue_IfUpdateSuccess(int id, string name)
    {
        // arrange
        var entityToDelete = new Department { Id = id, Name = name };
        mockDepartmentRepository.Setup(x => x.GetDepartmentWithEmployeesAsync(id)).ReturnsAsync(entityToDelete);
        mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

        // act
        var status = await departmentService.DeleteDepartmentAsync(id);

        // assert
        status.Should().BeTrue();

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to delete department with ID: {id}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, $"Department with ID {id} deleted successfully.", Times.Once());


        mockDepartmentRepository.Verify(x => x.GetDepartmentWithEmployeesAsync(It.Is<int>(x => x == id)), Times.Once());
        mockDepartmentRepository.Verify(x => x.Delete(It.Is<Department>(x => x.Id == id && x.Name == name)), Times.Once());
        mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(1011)]
    public async Task DeleteDepartmentAsync_ReturnsFalse_IfDepartmentWithIdUnavailable(int nonExistentId)
    {
        // arrange
        Department? nullDepartment = null;
        mockDepartmentRepository.Setup(x => x.GetDepartmentWithEmployeesAsync(nonExistentId)).ReturnsAsync(nullDepartment);

        // act
        var status = await departmentService.DeleteDepartmentAsync(nonExistentId);

        // assert
        status.Should().BeFalse();

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to delete department with ID: {nonExistentId}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Warning, $"Delete failed: Department with ID {nonExistentId} not found.", Times.Once());

        mockDepartmentRepository.Verify(x => x.GetDepartmentWithEmployeesAsync(It.Is<int>(x => x == nonExistentId)), Times.Once());
        mockDepartmentRepository.Verify(x => x.Delete(It.Is<Department>(x => x == nullDepartment)), Times.Never());
        mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());
    }

    [Theory]
    [InlineData(100, "HR")]
    [InlineData(1011, "IT")]
    public async Task DeleteDepartmentAsync_ReturnsFalse_WhenNoRowsAffected(int id, string name)
    {
        // arrange
        var entityToDelete = new Department { Id = id, Name = name };
        mockDepartmentRepository.Setup(x => x.GetDepartmentWithEmployeesAsync(id)).ReturnsAsync(entityToDelete);
        mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(0);                               // no rows affected

        // act
        var status = await departmentService.DeleteDepartmentAsync(id);

        // assert
        status.Should().BeFalse();

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to delete department with ID: {id}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"Department with ID {id} deletion unsuccessful.", Times.Once());

        mockDepartmentRepository.Verify(x => x.GetDepartmentWithEmployeesAsync(It.Is<int>(x => x == id)), Times.Once());
        mockDepartmentRepository.Verify(x => x.Delete(It.Is<Department>(x => x == entityToDelete)), Times.Once());
        mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());
    }

    [Theory]
    [InlineData(100, "HR")]
    [InlineData(1011, "IT")]
    public async Task DeleteDepartmentAsync_ReturnsFalse_WhenDepartmentHasAtleastOneEmployee(int id, string name)
    {
        // arrange
        var entityToDelete = new Department
        {
            Id = id,
            Name = name,
            Employees = new List<Employee> {
                new Employee { Id = 1, Email = "test@gmail.com", FirstName = "John", LastName = "Doe" }
            }
        };
        mockDepartmentRepository.Setup(x => x.GetDepartmentWithEmployeesAsync(id)).ReturnsAsync(entityToDelete);

        // act
        var status = await departmentService.DeleteDepartmentAsync(id);

        // assert
        status.Should().BeFalse();

        mockLogger.VerifyMessage(LogLevel.Information, $"Attempting to delete department with ID: {id}", Times.Once());
        mockLogger.VerifyMessage(LogLevel.Warning, $"Deletion of Department ID {id} restricted: Department still has {entityToDelete.Employees.Count()} associated employees.", Times.Once());

        mockDepartmentRepository.Verify(x => x.GetDepartmentWithEmployeesAsync(It.Is<int>(x => x == id)), Times.Once());
        mockDepartmentRepository.Verify(x => x.Delete(It.Is<Department>(x => x == entityToDelete)), Times.Never());
        mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());
    }
}
