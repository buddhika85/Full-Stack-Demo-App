using Emp.Core.Entities;
using Emp.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Emp.Application.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration configuration;
    private readonly ILogger<JwtService> logger;
    private const int TokenExpireHours = 5;


    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    /// <summary>
    /// User is already authenticated. Generate JWT token.
    /// </summary>
    /// <param name="user">Authenticated User object</param>
    /// <returns>JWT Token</returns>
    public string? GenerateJwtToken(User user)
    {
        logger.LogInformation("Attemtping to generate JWT Token for User '{Username}'", user.Username);
        try
        {
            var keyStr = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(keyStr))
            {
                throw new ArgumentNullException("Jwt:Key cannot be null or empty. Due to this unable to generate JWT auth token for users.");
            }
            var key = Encoding.ASCII.GetBytes(keyStr);

            var claims = PrepareClaimsList(user);
            var tokenDescriptor = GenerateTokenDescriptor(key, claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            logger.LogInformation("User '{Username}' JWT Token generated successfully.", user.Username);
            return tokenString;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "After successfuly authetication, User '{Username}' JWT Token generation failed.", user.Username);
            throw;
        }
    }

    private SecurityTokenDescriptor GenerateTokenDescriptor(byte[] key, List<Claim> claims)
    {
        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(TokenExpireHours),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };
    }

    private List<Claim> PrepareClaimsList(User user)
    {
        return new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role) // Role is already string from DB
                };
    }
}
