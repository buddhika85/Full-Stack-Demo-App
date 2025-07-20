using Microsoft.AspNetCore.Mvc;


namespace Emp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected ActionResult ValidationError(string field, string message)
    {
        var problemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            [field] = new string[] { message },
        });
        return new BadRequestObjectResult(problemDetails);
    }

    protected ActionResult NotFoundError(string detail, string title = "Not Found")
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = StatusCodes.Status404NotFound
        };
        return NotFound(problemDetails);
    }

    protected ActionResult InternalServerError(string detail, string title = "Internal Server Error")
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = StatusCodes.Status500InternalServerError
        };
        return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
    }

    protected ActionResult ConflictError(string detail, string title = "Conflict Error")
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = StatusCodes.Status409Conflict
        };
        return Conflict(problemDetails);
    }
}
