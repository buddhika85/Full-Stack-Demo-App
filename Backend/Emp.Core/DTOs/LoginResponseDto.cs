namespace Emp.Core.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!; // Contains Id, Username, Role, IsActive
}
