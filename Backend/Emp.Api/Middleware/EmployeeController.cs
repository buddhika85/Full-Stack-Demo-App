
using Emp.Api.Controllers;
using Emp.Core.Interfaces.Services;
using Microsoft.AspNetCore.OutputCaching;

namespace Emp.Api.Middleware;

public class EmployeeController : BaseController
{
    private readonly IEmployeeService employeeService;
    private readonly ILogger<EmployeeController> logger;
    private readonly IOutputCacheStore outputCacheStore;
    private const string cacheTag = "employees";

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger,
            IOutputCacheStore outputCacheStore)
    {
        this.employeeService = employeeService;
        this.logger = logger;
        this.outputCacheStore = outputCacheStore;
    }
}
