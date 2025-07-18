using Emp.Api.Controllers;
using Emp.Core.DTOs;
using Emp.Core.Interfaces.Services;
using Emp.XUnitTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Moq;

namespace Emp.XUnitTests.Controllers;

public class DepartmentControllerTests
{
    private readonly Mock<IDepartmentService> mockDepartmentService;
    private readonly Mock<ILogger<DepartmentController>> mockLogger;
    private readonly Mock<IOutputCacheStore> mockOutputCacheStore;

    private readonly DepartmentController departmentController;

    public DepartmentControllerTests()
    {
        mockDepartmentService = new Mock<IDepartmentService>();
        mockLogger = new Mock<ILogger<DepartmentController>>();
        mockOutputCacheStore = new Mock<IOutputCacheStore>();

        departmentController = new DepartmentController(mockDepartmentService.Object, mockLogger.Object, mockOutputCacheStore.Object);
    }

    [Fact]
    public async Task GetDepartments_ReturnsOkWithAllDepartments_WhenCalled()
    {
        // arrange
        var testData = new List<DepartmentDto> {
            new DepartmentDto { Id = 1, Name= "HR"},
            new DepartmentDto { Id = 2, Name= "IT"}
        };
        mockDepartmentService.Setup(x => x.GetAllDepartmentsAsync()).ReturnsAsync(testData);

        // act
        var result = await departmentController.GetDepartments();

        // assert
        mockDepartmentService.Verify(x => x.GetAllDepartmentsAsync(), Times.Once());
        result.Should().NotBeNull();
        result.Should().BeOfType<ActionResult<IEnumerable<DepartmentDto>>>();

        mockLogger.VerifyMessage(LogLevel.Information, "API: GetDepartments endpoint called (cached).", Times.Once());

        // is it a OkObjectResult?
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;

        // is ok result contains IEnumerable<DepartmentDto> ?
        var returedDepartments = okResult.Value.Should().BeAssignableTo<IEnumerable<DepartmentDto>>().Subject;
        returedDepartments.Should().NotBeNull();
        returedDepartments.Should().HaveCount(testData.Count);
        returedDepartments.Should().BeEquivalentTo(testData);
    }
}
