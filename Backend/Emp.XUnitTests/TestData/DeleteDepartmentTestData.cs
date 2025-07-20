
using Emp.Core.DTOs;

namespace Emp.XUnitTests.TestData;

public class DeleteDepartmentTestData : BaseTestData
{
    public override IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 1, new DepartmentDto { Id = 1, Name = "Human Resources" } };
        yield return new object[] { 2, new DepartmentDto { Id = 2, Name = "Engineering" } };
        yield return new object[] { 300, new DepartmentDto { Id = 300, Name = "Marketing" } };
        yield return new object[] { 17, new DepartmentDto { Id = 17, Name = "Sales" } };
    }
}

