
namespace Emp.Api.Middleware;

public class CustomExceptionMiddleware : IMiddleware
{
    private readonly ILogger<CustomExceptionMiddleware> logger;
    private readonly IHostEnvironment env;

    public CustomExceptionMiddleware(ILogger<CustomExceptionMiddleware> logger, IHostEnvironment env)
    {
        this.logger = logger;
        this.env = env;
    }


    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An Unhandled Exception occured: {Message} {InnerException}", ex.Message, ex.InnerException);

            context.Response.ContentType = "application/json";

            // exception type 
            context.Response.StatusCode = ex switch
            {
                InvalidOperationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorDetails = new
            {
                status = context.Response.StatusCode,
                title = "An unexpected error occurred.",
                message = ex.Message,
                innerException = env.IsDevelopment() ? ex.InnerException?.Message : null,
                stackTrace = env.IsDevelopment() ? ex.StackTrace : null
            };

            await context.Response.WriteAsJsonAsync(errorDetails);

        }
    }
}
