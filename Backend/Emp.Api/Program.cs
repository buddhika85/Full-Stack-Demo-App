using Emp.Api.Configurations;
using Emp.Api.Filters;
using Emp.Api.Middleware;
using Emp.Api.PollyPolicies;
using Emp.Application.Services;
using Emp.Core;
using Emp.Core.Interfaces.Repositories;
using Emp.Core.Interfaces.Services;
using Emp.Infrastructure;
using Emp.Infrastructure.Data;
using Emp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// services
builder.Services.AddScoped<CustomExceptionMiddleware>();     // scoped as it needs be accessed by multiple threads of same request
builder.Services.AddScoped<ConsoleLoggerFilter>();

//builder.Services.AddSingleton<IInMemoryRepository, InMemoryRepository>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>

    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddScoped<IPasswordHasherService, BCryptPasswordHasherService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IUserService, UserService>();

// read azure settings to AzureIntegrationSettings
builder.Services.Configure<AzureIntegrationSettings>(
    builder.Configuration.GetSection("AzureIntegration"));


builder.Services.AddSingleton<ClientPolicy>();
builder.Services.AddHttpClient("ExponentialBackOffForPost")
    .AddPolicyHandler((sp, request) =>
    {
        var policy = sp.GetRequiredService<ClientPolicy>();             // get ClientPolicy singleton object from DI container
        return request.Method == HttpMethod.Post
            ? policy.ExponentialHttpRetryPolicy
            : policy.LinearHttpRetryPolicy;
    });

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var keyStr = jwtSettings["Key"];
if (string.IsNullOrWhiteSpace(keyStr))
{
    Log.Error("Startup aborted: 'Jwt:Key' is missing from configuration.");
    throw new InvalidOperationException("JWT secret key is missing in configuration. Please set 'Jwt:Key' in appsettings or environment variables.");
}

var key = Encoding.ASCII.GetBytes(keyStr);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{

    // standard token validation checks from HTTP Request Header - Authoroization 

    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],                    // was the token issued by this API ?
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],                // was this token intended for Angular App?
        ValidateLifetime = true,                                // Validate token expiry
        ClockSkew = TimeSpan.Zero                               // No leeway for token expiry
    };


    // Check for blacklisted tokens
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            // Get the raw token from the request
            string? accessToken = context.HttpContext.Request.Headers["Authorization"]
                                    .ToString()
                                    .Replace("Bearer ", "");

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                context.Fail("Token is null.");                                 // Explicitly fail authentication
                return;
            }

            var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
            if (jwtService.IsBlacklistedToken(accessToken))
            {
                context.Fail("Token has been revoked/blacklisted.");            // Explicitly fail authentication
                return;
            }

            // If not blacklisted or not null, continue with normal token processing
            await Task.CompletedTask;
        }
    };
});




// CORS
var allowedOrigins = builder.Configuration["AllowedOrigins"]?.ToString().Split(",") ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();        // Allow credentials for JWT

        //policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();      // allowing any one to access this API
    });
});



// Seri Log Logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/apiLog-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Services.AddSerilog();



builder.Services.AddControllers();

builder.Services.AddOpenApi();
// using swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// caching
builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(15);
});






var app = builder.Build();  // RUNS ONCE PER APPLICATION CYCLE - This compiles everything into a runnable app



using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Apply any pending migrations
    await dbContext.Database.MigrateAsync();

    await ApplicationDbSeeder.SeedAsync(dbContext); // Seed initial data here
}



#region MIDDLEWARE_PIPELINE


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//app.UseDeveloperExceptionPage();            // using built in developer exception page - stack trace, error code scection, route values, headers, cookies

app.UseMiddleware<CustomExceptionMiddleware>(); // custom middleware to tackle exceptions

// using swagger
app.UseSwagger();
app.UseSwaggerUI();
//}
//else
//{
//    // redirect to production exception page
//    app.UseExceptionHandler("/error");
//}

app.UseHttpsRedirection();



app.UseRouting();                       // Route resolution

app.UseCors();                          // adding ACCESS-CONTROL-ALLOW-ORIGIN header

app.UseOutputCache();                   // use outputcache middleware

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization(); // auth should be done before mapping controllers middleware


app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 404 && !response.HasStarted)
    {
        response.ContentType = "text/plain";
        await response.WriteAsync(
            $"404 from UseStatusCodePages: {context.HttpContext.Request.Path}");
    }
});



app.MapControllers();   // HIGH LEVEL MIDDLEWARE - Route registration and Filter pipeline execution, and then End Point Execution 


// Inline custom middleware for 404s --> no need as - app.UseStatusCodePages used
//app.Use(async (context, next) =>
//{
//    await next(); // Let the pipeline finish first

//    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
//    {
//        context.Response.ContentType = "text/plain";
//        await context.Response.WriteAsync(
//            $"404: {context.Request.Method} {context.Request.Path}");
//    }
//});


#endregion #region MIDDLEWARE_PIPELINE






app.Run();  // RUNS ONCE PER APPLICATION CYCLE - Hosts application on Kestral and it starts listening for HTTP Requests, DOES NOT EXECUTE PER REQUEST
