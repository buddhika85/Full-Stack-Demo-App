using Emp.Api.Filters;
using Emp.Api.Middleware;
using Emp.Application.Services;
using Emp.Core;
using Emp.Core.Interfaces.Repositories;
using Emp.Core.Interfaces.Services;
using Emp.Infrastructure;
using Emp.Infrastructure.Data;
using Emp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// services
builder.Services.AddScoped<CustomExceptionMiddleware>();     // scoped as it needs be accessed by multiple threads for multiple requests
builder.Services.AddScoped<ConsoleLoggerFilter>();

//builder.Services.AddSingleton<IInMemoryRepository, InMemoryRepository>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]);
});

builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IUserService, UserService>();


// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true, // Validate token expiry
        ClockSkew = TimeSpan.Zero // No leeway for token expiry
    };
});




// CORS
var allowedOrigins = builder.Configuration["AllowedOrgins"]?.ToString().Split(",") ?? [];
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
    await ApplicationDbSeeder.SeedAsync(dbContext); // Seed initial data here
}



#region MIDDLEWARE_PIPELINE


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();            // using built in developer exception page - stack trace, error code scection, route values, headers, cookies

    app.UseMiddleware<CustomExceptionMiddleware>(); // custom middleware to tackle exceptions

    // using swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // redirect to production exception page
    app.UseExceptionHandler("/error");
}

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
