using Emp.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Emp.Application.Services;

public class BCryptPasswordHasherService : IPasswordHasherService
{
    private readonly ILogger<BCryptPasswordHasherService> logger;

    public BCryptPasswordHasherService(ILogger<BCryptPasswordHasherService> logger)
    {
        this.logger = logger;
    }

    public string HashPassword(string password)
    {
        try
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in hashing password");
            throw;
        }
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in verifying password");
            throw;
        }
    }
}
