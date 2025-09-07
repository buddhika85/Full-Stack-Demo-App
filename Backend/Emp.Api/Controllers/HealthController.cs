using Microsoft.AspNetCore.Mvc;

namespace Emp.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("API is running");
}

