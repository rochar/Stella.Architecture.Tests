namespace Stella.Architecture.Tests.Exceptions;

[Serializable]
public class AssertTypeDependencyException(string message, Type currentType, Type referencedType)
    : Exception(message)
{
    public Type CurrentType { get; } = currentType;
    public Type ReferencedType { get; } = referencedType;
}