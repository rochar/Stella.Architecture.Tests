namespace Stella.Architecture.Tests;

[Serializable]
public class AssertArchitectureException(string message, params Exception[] assertArchitectureExceptions) : Exception(message)
{
    public Exception[] AssertArchitectureExceptions { get; } = assertArchitectureExceptions;
}