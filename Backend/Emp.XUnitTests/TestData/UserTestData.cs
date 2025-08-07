

using Emp.Core.Entities;
using Emp.Core.Enums;

namespace Emp.XUnitTests.TestData;

public class UserTestData : BaseTestData
{
    public override IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] {
            new User
            {
                Id = 1,
                Username = "admin@emp.com",
                Role = UserRoles.Admin.ToString(),
                IsActive = true,
                FirstName = "Admin",
                LastName = "User"
            }
        };
        yield return new object[] {
            new User
            {
                Id = 2,
                Username = "staff@emp.com",
                Role = UserRoles.Staff.ToString(),
                IsActive = true,
                FirstName = "Staff",
                LastName = "Member"
            }
        };
    }
}
