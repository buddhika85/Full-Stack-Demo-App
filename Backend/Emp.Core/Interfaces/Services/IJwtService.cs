using Emp.Core.Entities;

namespace Emp.Core.Interfaces.Services;

public interface IJwtService
{
    public string? GenerateJwtToken(User user);
}
