using Microsoft.AspNetCore.Mvc.Filters;

namespace Emp.Api.Filters;

public class ConsoleLoggerFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        Console.WriteLine(">>-------------------------- CONSOLE LOGGER FILTER -------------------------->>");
        Console.WriteLine($"Action Method {context.ActionDescriptor.DisplayName} Starting at {DateTime.Now}");
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        Console.WriteLine(">>-------------------------- CONSOLE LOGGER FILTER -------------------------->>");
        Console.WriteLine($"Action Method {context.ActionDescriptor.DisplayName} Finished at {DateTime.Now}");
    }
}
