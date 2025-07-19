using Emp.Api.Controllers;
using Emp.Core.DTOs;
using Emp.Core.Interfaces.Services;
using Emp.XUnitTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        okResult.StatusCode.Should().Be(200);

        // is ok result contains IEnumerable<DepartmentDto> ?
        var returedDepartments = okResult.Value.Should().BeAssignableTo<IEnumerable<DepartmentDto>>().Subject;
        returedDepartments.Should().NotBeNull();
        returedDepartments.Should().HaveCount(testData.Count);
        returedDepartments.Should().BeEquivalentTo(testData);
    }

    [Fact]
    public async Task GetDepartments_ReturnsInternalServerError_OnException()
    {
        // arrange        
        var exepctedProblemDetail = new ProblemDetails
        {
            Title = "Internal Server Error",
            Detail = "Error occured while retreiving departments",
            Status = StatusCodes.Status500InternalServerError
        };
        mockDepartmentService.Setup(x => x.GetAllDepartmentsAsync()).ThrowsAsync(new Exception());

        // act
        var result = await departmentController.GetDepartments();

        // assert
        result.Should().NotBeNull();
        result.Should().BeOfType(typeof(ActionResult<IEnumerable<DepartmentDto>>));

        var internalServerErrorResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        internalServerErrorResult.StatusCode.Should().Be(500);

        var probelmDetail = internalServerErrorResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        probelmDetail.Should().BeEquivalentTo(exepctedProblemDetail);

        mockLogger.VerifyMessage(LogLevel.Information, "API: GetDepartments endpoint called (cached).", Times.Never());
        mockLogger.VerifyMessage(LogLevel.Error, "Error occured while retreiving departments", Times.Once());

        mockDepartmentService.Verify(x => x.GetAllDepartmentsAsync(), Times.Once());
    }

    [Theory]
    [InlineData(1, "HR")]
    [InlineData(2, "IT")]
    public async Task GetDepartment_ReturnsOkWithDepartment_WhenDepartmentWithIdExistent(int id, string name)
    {
        // arrange
        var deprtmentDto = new DepartmentDto { Id = id, Name = name };
        mockDepartmentService.Setup(x => x.GetDepartmentByIdAsync(id)).ReturnsAsync(deprtmentDto);

        // act
        var result = await departmentController.GetDepartment(id);

        // assert
        var okObjectResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okObjectResult.StatusCode.Should().Be(200);
        var returnedDto = okObjectResult.Value.Should().BeAssignableTo<DepartmentDto>().Subject;
        returnedDto.Should().BeEquivalentTo(deprtmentDto);

        mockDepartmentService.Verify(x => x.GetDepartmentByIdAsync(It.Is<int>(x => x == id)), Times.Once());
    }

    [Fact]
    public async Task GetDepartment_ReturnsNotFoundResult_WhenDepartmentWithIdNonExistent()
    {
        // arrange

        // act

        // assert
    }
}
