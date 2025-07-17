namespace Emp.XUnitTests.Helpers;

public static class Helpers
{
    public static bool CheckForLogMessage(object value, string expectedLogMessage)
    {
        return value?.ToString() is string message
            && message.Contains(expectedLogMessage);
    }
}
