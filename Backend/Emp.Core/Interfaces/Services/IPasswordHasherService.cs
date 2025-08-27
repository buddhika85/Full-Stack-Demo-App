namespace Emp.Core.Interfaces.Services;

public interface IPasswordHasherService
{
    public bool VerifyPassword(string password, string hashedPassword);
    public string HashPassword(string password);
}
