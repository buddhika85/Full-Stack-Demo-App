using Emp.Core.DTOs;
using Emp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Emp.Api.Controllers;


public class HomeController : BaseController
{
    private readonly IDepartmentService departmentService;
    private readonly ILogger<HomeController> logger;
    private const string cacheTag = "landing";

    public HomeController(IDepartmentService departmentService, ILogger<HomeController> logger)
    {
        this.departmentService = departmentService;
        this.logger = logger;
    }

    [EndpointSummary("Gets Landing Page Content - Department names and their employee counts")]
    [AllowAnonymous]
    [HttpGet]
    [OutputCache(Tags = [cacheTag])]       // this is configured to keep 15 seconds in Program.cs
    public async Task<ActionResult<LandingDto>> LandingContent()
    {
        try
        {
            LandingDto landingDto = new();
            landingDto.Departments = await departmentService.GetAllDepartmentsWithEmpCountsAsync();
            logger.Log(LogLevel.Information, "API: Landing page endpoint called.");
            return Ok(landingDto);
        }
        catch (Exception ex)
        {
            const string error = "Error occured while retreiving landing page content";
            logger.LogError(ex, error);
            return InternalServerError(error);
        }
    }
}
