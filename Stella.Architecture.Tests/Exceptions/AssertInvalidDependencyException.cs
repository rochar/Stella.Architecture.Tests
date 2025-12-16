namespace Stella.Architecture.Tests.Exceptions;

[Serializable]
public class AssertInvalidDependencyException(string message, Type currentType, Type referencedType)
    : AssertArchitectureException(message)
{
    public Type CurrentType { get; } = currentType;
    public Type ReferencedType { get; } = referencedType;
}