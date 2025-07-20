
using Emp.Core.DTOs;

namespace Emp.XUnitTests.TestData;

public class CreateDepartmentTestData : BaseTestData
{
    public override IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new CreateDepartmentDto { Name = "Human Resources" }, new DepartmentDto { Id = 1, Name = "Human Resources" } };
        yield return new object[] { new CreateDepartmentDto { Name = "Engineering" }, new DepartmentDto { Id = 200, Name = "Engineering" } };
        yield return new object[] { new CreateDepartmentDto { Name = "Marketing" }, new DepartmentDto { Id = 0, Name = "Marketing" } };
        yield return new object[] { new CreateDepartmentDto { Name = "Sales" }, new DepartmentDto { Id = 4, Name = "Sales" } };
    }
}
