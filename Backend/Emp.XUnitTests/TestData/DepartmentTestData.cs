using Emp.Core.Entities;

namespace Emp.XUnitTests.TestData;

public class DepartmentTestData : BaseTestData
{
    public override IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new Department { Id = 1, Name = "Human Resources" } };
        yield return new object[] { new Department { Id = 2, Name = "Engineering" } };
        yield return new object[] { new Department { Id = 3, Name = "Marketing" } };
        yield return new object[] { new Department { Id = 4, Name = "Sales" } };
    }
}
