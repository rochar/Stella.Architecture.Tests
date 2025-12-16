namespace Stella.Architecture.Tests;

[Serializable]
public class AssertArchitectureException(string message) : Exception(message)
{
}