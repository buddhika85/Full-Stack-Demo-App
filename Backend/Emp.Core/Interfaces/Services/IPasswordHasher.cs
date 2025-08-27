namespace Emp.Core.Interfaces.Services;

public interface IPasswordHasher
{
    public bool VerifyPassword(string password, string hashedPassword);
    public string HashPassword(string password);
}
