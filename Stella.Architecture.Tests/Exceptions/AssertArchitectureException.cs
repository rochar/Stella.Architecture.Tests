namespace Stella.Architecture.Tests.Exceptions;

[Serializable]
public class AssertArchitectureException(string message, params Exception[] assertExceptions) : Exception(message + GetDetails(assertExceptions))
{
    public Exception[] AssertExceptions { get; } = assertExceptions;

    private static string GetDetails(Exception[] exceptions)
    {
        if (exceptions.Length == 0)
            return string.Empty;
        var details = string.Join("\n", exceptions.Select(e => e.Message));
        return "\nAssertExceptions:\n" + details;
    }
}