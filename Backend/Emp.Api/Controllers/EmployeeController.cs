using Emp.Core.DTOs;
using Emp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Emp.Api.Controllers;

public class EmployeeController : BaseController
{
    private readonly IEmployeeService employeeService;
    private readonly ILogger<EmployeeController> logger;
    private readonly IOutputCacheStore outputCacheStore;
    private const string cacheTag = "employees";

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger, IOutputCacheStore outputCacheStore)
    {
        this.employeeService = employeeService;
        this.logger = logger;
        this.outputCacheStore = outputCacheStore;
    }

    [EndpointSummary("Gets all employees")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    [HttpGet]
    [OutputCache(Tags = [cacheTag])]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        try
        {
            return Ok(await employeeService.GetAllEmployeesAsync());
        }
        catch (Exception ex)
        {
            const string error = "Error occured while retreiving employees";
            logger.LogError(ex, error);
            return InternalServerError(error);
        }
    }


    // GetEmployee

    // CreateEmployees

    // UpdateEmployees

    // DeleteEmployees
}
