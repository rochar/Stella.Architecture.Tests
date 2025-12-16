namespace Stella.Architecture.Tests;

[Serializable]
public class AssertInvalidDependencyException(string message, Type currentType, Type referencedType)
    : AssertArchitectureException(message)
{
    public Type CurrentType { get; } = currentType;
    public Type ReferencedType { get; } = referencedType;
}