namespace Stella.Architecture.Tests.Exceptions;

[Serializable]
public class AssertArchitectureException(string message, params Exception[] assertArchitectureExceptions) : Exception(message)
{
    public Exception[] AssertArchitectureExceptions { get; } = assertArchitectureExceptions;
}