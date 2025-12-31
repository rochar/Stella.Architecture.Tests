namespace Stella.Architecture.Tests.Exceptions;

[Serializable]
public class AssertArchitectureException(string message, params AssertInvalidDependencyException[] assertInvalidDependencyExceptions) : Exception(message)
{
    public AssertInvalidDependencyException[] AssertInvalidDependencyExceptions { get; } = assertInvalidDependencyExceptions;
}