
using Emp.Core.Entities;

namespace Emp.XUnitTests.TestData;

public class EmployeeTestData : BaseTestData
{
    public override IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new Employee { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", DepartmentId = 2 } };
        yield return new object[] { new Employee { FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", DepartmentId = 1 } };
        yield return new object[] { new Employee { FirstName = "Peter", LastName = "Jones", Email = "peter.jones@example.com", DepartmentId = 2 } };
    }
}
