namespace Emp.Core.DTOs;

public class LogoutResponseDto
{
    public bool LoggedOut { get; set; }
    public required string Username { get; set; }
    public string? LogoutMessage => LoggedOut ? $"{Username}: Logged out successfully (token blacklisted) at {DateTime.Now}" : null;
}
