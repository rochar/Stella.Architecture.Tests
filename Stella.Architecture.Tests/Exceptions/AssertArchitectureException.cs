namespace Stella.Architecture.Tests.Exceptions;

[Serializable]
public class AssertArchitectureException(string message, params Exception[] assertExceptions) : Exception(message)
{
    public Exception[] AssertExceptions { get; } = assertExceptions;
}