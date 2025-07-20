
using Emp.Core.DTOs;

namespace Emp.XUnitTests.TestData;

public class UpdateDepartmentTestData : BaseTestData
{
    public override IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 1, new DepartmentDto { Id = 1, Name = "Human Resources" }, new UpdateDepartmentDto { Id = 1, Name = "HR" } };
        yield return new object[] { 2, new DepartmentDto { Id = 2, Name = "Engineering" }, new UpdateDepartmentDto { Id = 2, Name = "Eg" } };
        yield return new object[] { 300, new DepartmentDto { Id = 300, Name = "Marketing" }, new UpdateDepartmentDto { Id = 300, Name = "MKT" } };
        yield return new object[] { 17, new DepartmentDto { Id = 17, Name = "Sales" }, new UpdateDepartmentDto { Id = 17, Name = "SL" } };
    }
}
