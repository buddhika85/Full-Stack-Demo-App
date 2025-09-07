using Emp.Core.DTOs;
using Emp.Core.Enums;
using Emp.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Emp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [HttpGet("/api/home")]
    [HttpGet("landing-content")]
    [OutputCache(Tags = [cacheTag])]       // this is configured to keep 15 seconds in Program.cs
    public async Task<ActionResult<LandingDto>> LandingContent()
    {
        try
        {
            LandingDto landingDto = new();
            landingDto.Departments = await departmentService.GetAllDepartmentsWithEmpCountsAsync();
            logger.LogInformation("API: Landing page endpoint called.");
            return Ok(landingDto);
        }
        catch (Exception ex)
        {
            const string error = "Error occured while retreiving landing page content";
            logger.LogError(ex, error);
            return InternalServerError(error);
        }
    }

    [EndpointSummary("Returns Allowed Origins By Backend API")]
    ////[Authorize(Roles = $"{nameof(UserRoles.Admin)},{nameof(UserRoles.Staff)}")]
    [HttpGet("allowed-origins")]
    [HttpGet("/api/test1")]
    public ActionResult<IEnumerable<string>> GetAllowedOriginList(IConfiguration configuration)
    {
        try
        {
            logger.LogInformation("API: Get Allowed Origin List endpoint called.");
            var strAllowedOrigins = configuration["AllowedOrigins"]?.ToString();
            if (string.IsNullOrWhiteSpace(strAllowedOrigins))
            {
                const string error = "AllowedOrigins - setting is not available";
                throw new ArgumentException(error);
            }
            var allowedOriginList = strAllowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (allowedOriginList == null || allowedOriginList.Count() == 0)
            {
                const string error = "AllowedOrigins - is not set - but setting is available";
                throw new ArgumentException(error);
            }


            logger.LogInformation("Raw AllowedOrigins string: {origins}", strAllowedOrigins);
            logger.LogInformation("Parsed AllowedOrigins list: {@list}", allowedOriginList);
            return Ok(allowedOriginList);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, ex.Message);
            return InternalServerError($"Internal Server Error occurred while reading Allowed-Orgin for CORS - {ex.Message}");
        }
        catch (Exception ex)
        {
            const string error = "Error occurred while reading Allowed-Orgin for CORS";
            logger.LogError(ex, error);
            return InternalServerError(error);
        }
    }

}
