using System.Collections;

namespace Emp.XUnitTests.TestData;

public abstract class BaseTestData : IEnumerable<object[]>
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public abstract IEnumerator<object[]> GetEnumerator();
}
