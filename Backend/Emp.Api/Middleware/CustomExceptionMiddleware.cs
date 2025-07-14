
namespace Emp.Api.Middleware;

public class CustomExceptionMiddleware : IMiddleware
{
    private readonly ILogger<CustomExceptionMiddleware> logger;

    public CustomExceptionMiddleware(ILogger<CustomExceptionMiddleware> logger)
    {
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An Exception occured: {Message} {InnerException}", ex.Message, ex.InnerException);
        }
    }
}
