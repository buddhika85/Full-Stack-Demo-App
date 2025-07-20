using Emp.Api.Controllers;
using Emp.Core.DTOs;
using Emp.Core.Interfaces.Services;
using Emp.XUnitTests.Helpers;
using Emp.XUnitTests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
    private const string cacheTag = "departments";

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

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task GetDepartment_ReturnsNotFoundResult_WhenDepartmentWithIdNonExistent(int nonExistentId)
    {
        // arrange
        DepartmentDto? nullDepartment = null;
        mockDepartmentService.Setup(x => x.GetDepartmentByIdAsync(nonExistentId)).ReturnsAsync(nullDepartment);

        // act
        var result = await departmentController.GetDepartment(nonExistentId);

        // assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);

        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Not Found");
        problemDetails.Detail.Should().Be($"Department with ID {nonExistentId} not found");

        mockLogger.VerifyMessage(LogLevel.Warning, $"Department with ID {nonExistentId} not found", Times.Once());

        mockDepartmentService.Verify(x => x.GetDepartmentByIdAsync(It.Is<int>(x => x == nonExistentId)), Times.Once());
    }

    [Fact]
    public async Task GetDepartment_ReturnsInternalServerError_OnException()
    {
        // arrange
        var testId = 1;
        mockDepartmentService.Setup(x => x.GetDepartmentByIdAsync(testId)).ThrowsAsync(new Exception("Test Exception"));

        // act
        var result = await departmentController.GetDepartment(testId);

        // assert
        var internalServerErrorResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        var problemDetails = internalServerErrorResult.Value.Should().BeAssignableTo<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(500);
        problemDetails.Title.Should().Be("Internal Server Error");
        problemDetails.Detail.Should().Be($"An error occurred while retrieving a department by ID {testId}.");

        mockDepartmentService.Verify(x => x.GetDepartmentByIdAsync(It.Is<int>(x => x == testId)), Times.Once);
        mockLogger.VerifyMessage(LogLevel.Error, $"Error occured while retreiving a department by Id {testId}", Times.Once());
    }

    [Theory]
    [ClassData(typeof(CreateDepartmentTestData))]
    public async Task CreateDepartment_ReturnsCreatedResult_WhenSuccessful(CreateDepartmentDto createDepartmentDto, DepartmentDto departmentDtoExpected)
    {
        // arrange       
        const string logMessage = $"API: CreateDepartment endpoint called (evicted cache on cache tag {cacheTag}).";
        mockDepartmentService.Setup(x => x.CreateDepartmentAsync(createDepartmentDto)).ReturnsAsync(departmentDtoExpected);

        // act
        var result = await departmentController.CreateDepartment(createDepartmentDto);

        // assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be("GetDepartment");
        createdResult.RouteValues.Should().HaveCount(1);
        createdResult.RouteValues["id"].Should().BeEquivalentTo(departmentDtoExpected.Id);


        var departmentDtoActual = createdResult.Value.Should().BeAssignableTo<DepartmentDto>().Subject;
        departmentDtoActual.Should().NotBeNull();
        departmentDtoActual.Should().BeEquivalentTo(departmentDtoExpected);

        mockDepartmentService.Verify(x => x.CreateDepartmentAsync(It.Is<CreateDepartmentDto>(x => x == createDepartmentDto)), Times.Once());
        mockOutputCacheStore.Verify(x => x.EvictByTagAsync(cacheTag, default), Times.Once());
        mockLogger.VerifyMessage(LogLevel.Information, logMessage, Times.Once());
    }

    [Theory]
    [InlineData("HR")]
    [InlineData("IT")]
    public async Task CreateDepartment_ReturnsInternalServerError_WhenServiceReturnsNull(string deptName)
    {
        // arrange
        var expectedProblemDetail = new ProblemDetails
        {
            Detail = $"Error occured while creating a department with name {deptName}",
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error"
        };
        var createDepartmentDto = new CreateDepartmentDto { Name = deptName };
        mockDepartmentService.Setup(x => x.CreateDepartmentAsync(createDepartmentDto)).ReturnsAsync((DepartmentDto?)null);

        // act
        var result = await departmentController.CreateDepartment(createDepartmentDto);

        // assert
        var internalServerErrorResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        internalServerErrorResult.StatusCode.Should().Be(500);

        var problemDetails = internalServerErrorResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Should().BeEquivalentTo(expectedProblemDetail);

        mockDepartmentService.Verify(x => x.CreateDepartmentAsync(It.Is<CreateDepartmentDto>(x => x == createDepartmentDto)), Times.Once());
        mockLogger.VerifyMessage(LogLevel.Error, $"Error occured while creating a department with name {createDepartmentDto.Name}", Times.Once());
        mockOutputCacheStore.Verify(x => x.EvictByTagAsync(cacheTag, default), Times.Never());
    }
}
